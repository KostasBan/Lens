namespace KostasBan.Lens
{
    public interface ILensIdentifiedSectionProvider : ILensSectionProvider
    {
        string SectionId { get; }
    }
}
