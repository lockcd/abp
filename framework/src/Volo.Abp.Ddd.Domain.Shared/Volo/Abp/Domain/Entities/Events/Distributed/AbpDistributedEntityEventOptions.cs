namespace Volo.Abp.Domain.Entities.Events.Distributed;

public class AbpDistributedEntityEventOptions
{
    public IAutoEntityDistributedEventSelectorList AutoEventSelectors { get; }

    public IAutoEntityDistributedEventSelectorList IgnoreEventSelectors { get; }

    public EtoMappingDictionary EtoMappings { get; set; }

    public AbpDistributedEntityEventOptions()
    {
        AutoEventSelectors = new AutoEntityDistributedEventSelectorList();
        IgnoreEventSelectors = new AutoEntityDistributedEventSelectorList();
        EtoMappings = new EtoMappingDictionary();
    }
}
