namespace Example.Lib
{
    public interface IRoot : Csla.IBusinessBase, ObjectPortal.IDPBusinessObject
    {
        IBusinessItemList BusinessItemList { get; set; }

    }
}