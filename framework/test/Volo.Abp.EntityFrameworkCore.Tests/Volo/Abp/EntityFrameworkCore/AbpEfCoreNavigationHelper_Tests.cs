using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.TestApp.Domain;
using Xunit;

namespace Volo.Abp.EntityFrameworkCore;

public class AbpEfCoreNavigationHelper_Tests : EntityFrameworkCoreTestBase
{
    private readonly IRepository<Blog, Guid> _blogRepository;
    private readonly IRepository<Post, Guid> _postRepository;

    public AbpEfCoreNavigationHelper_Tests()
    {
        _blogRepository = GetRequiredService<IRepository<Blog, Guid>>();
        _postRepository = GetRequiredService<IRepository<Post, Guid>>();
    }

    [Fact]
    public async Task Performance_Test()
    {
        var stopWatch = Stopwatch.StartNew();

        await WithUnitOfWorkAsync(async () =>
        {
            for (var i = 0; i < 5 * 1000; i++)
            {
                await _blogRepository.InsertAsync(
                    new Blog(Guid.NewGuid())
                    {
                        Name = "Blog" + i,
                        BlogPosts = new List<BlogPost>
                        {
                            new BlogPost
                            {
                                Post = new Post(Guid.NewGuid())
                                {
                                    Title = "Post" + i
                                }
                            }
                        }
                    });
            }
        });

        stopWatch.Stop();
        stopWatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(106));

        stopWatch.Restart();
        var blogs = await _blogRepository.GetListAsync(includeDetails: true);
        var posts = await _postRepository.GetListAsync(includeDetails: true);
        blogs.Count.ShouldBe(5 * 1000);
        blogs.SelectMany(x => x.BlogPosts.Select(y => y.Post)).Count().ShouldBe(5 * 1000);
        posts.Count.ShouldBe(5 * 1000);
        posts.SelectMany(x => x.BlogPosts.Select(y => y.Blog)).Count().ShouldBe(5 * 1000);

        stopWatch.Stop();
        stopWatch.Elapsed.ShouldBeLessThan(TimeSpan.FromSeconds(1));
    }
}
