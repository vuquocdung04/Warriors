#if UNITY_EDITOR
using System.Collections.Generic;

public interface ISheetImporter
{
    string Name { get; }                       
    object Parse(List<List<string>> rows);   
    string AnchorColumn { get; }    
}
#endif