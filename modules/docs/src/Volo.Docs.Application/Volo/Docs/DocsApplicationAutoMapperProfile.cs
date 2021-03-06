﻿using AutoMapper;
using Volo.Docs.Documents;
using Volo.Docs.Projects;
using Volo.Abp.AutoMapper;

namespace Volo.Docs
{
    public class DocsApplicationAutoMapperProfile : Profile
    {
        public DocsApplicationAutoMapperProfile()
        {
            CreateMap<Project, ProjectDto>().Ignore(x=>x.Languages);
            CreateMap<VersionInfo, VersionInfoDto>();
            CreateMap<Document, DocumentWithDetailsDto>()
                .Ignore(x => x.Project).Ignore(x => x.Contributors).Ignore(x => x.CurrentLanguageCode);
            CreateMap<DocumentContributor, DocumentContributorDto>();
            CreateMap<DocumentResource, DocumentResourceDto>();
        }
    }
}