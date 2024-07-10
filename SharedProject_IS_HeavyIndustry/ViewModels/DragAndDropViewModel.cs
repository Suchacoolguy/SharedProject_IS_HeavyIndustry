using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using SharedProject_IS_HeavyIndustry.Models;
using SharedProject_IS_HeavyIndustry.Services;
using SharedProject_IS_HeavyIndustry.Views;

namespace SharedProject_IS_HeavyIndustry.ViewModels;

public class DragAndDropViewModel
{
    public static ObservableCollection<RawMaterial> ArrangedRawMaterials { get; set; }
    public static ObservableCollection<Part> OverSizeParts { get; set; }
    public static string ArrangementType { get; set; } = "Min Raw Material Type";
    public RawMaterial CurrentRawMaterial { get; set; }
    public static ObservableCollection<Part> TempPartList { get; set; } = new ObservableCollection<Part>();
    public static string key { get; set; }
    public Part DraggedPart { get; set; }
    private Point _ghostPosition = new(0,0);
    private readonly Point _mouseOffset = new(-5, -5);

    public DragAndDropViewModel(ObservableCollection<RawMaterial> arrangedRawMaterials,
        ObservableCollection<Part> overSizeParts, string key)
    {
        ArrangedRawMaterials = new ObservableCollection<RawMaterial>(arrangedRawMaterials);
        OverSizeParts = new ObservableCollection<Part>(overSizeParts);
        DragAndDropViewModel.key = key;
    }

    public static void UpdateRawMaterial(RawMaterial from, RawMaterial to, Part part)
    {
        int index_part = 0;
        
        if (part != null)
        {
            index_part = from.PartsInside.IndexOf(part);
        }

        if (from == null)
        {
            if (to == null)
            {
                if (part == null)
                {
                    
                }
                else    // part != null일 때
                {
                    
                }
            }
            else    // to != null일 때
            {
                if (part == null)
                {
                    Console.WriteLine("우측에서 좌측으로 드랍");
                }
                else    // part != null일 때
                {
                    
                }
            }
        }
        else    // from != null일 때
        {
            if (to == null)
            {
                if (part == null)
                {
                    
                }
                else    // part != null일 때
                {
                    // 빈 공간에 파트를 드랍했을 때 새로운 원자재를 생성해서 그곳으로 파트를 이동
                    
                    Console.WriteLine("UpdateRawMaterial - from: null, to: null, part: not null");
                    List<int> lengthOptions = GetLengthOptionsRawMaterial();
                    int bestLength = FindBestSizeRawMaterial(lengthOptions, part);
            
                    // 새로운 원자재 생성하고 그곳에 파트 추가
                    RawMaterial newRawMaterial = new RawMaterial(bestLength);
                    newRawMaterial.insert_part(part);
                    ArrangedRawMaterials.Insert(ArrangedRawMaterials.Count, newRawMaterial);
                    
                    // 원래 파트가 있던 원자재에서 파트를 제거.
                    from.remove_part_at(index_part);
                    // 파트 제거 후 남은 파트가 없다면 해당 원자재 제거
                    if (from.PartsInside.Count == 0)
                    {
                        ArrangedRawMaterials.Remove(from);
                    }
                }
            }
            else    // to != null일 때
            {
                if (part == null)
                {
                    
                }
                else    // part != null일 때
                {
                    to.insert_part(part);
                    from.remove_part_at(index_part);
                    if (from.PartsInside.Count == 0)
                    {
                        ArrangedRawMaterials.Remove(from);
                    }
                    Console.WriteLine("UpdateRawMaterial - from: not null, to: not null, part: not null");
                }
            }
        }
        
        MainWindowViewModel.UpdateRawMaterialSet(ArrangedRawMaterials, key);
    }

    public static int FindBestSizeRawMaterial(List<int> lengthOptions, Part part)
    {
        int bestLength = lengthOptions.Max(); 
        foreach (var len in lengthOptions)
        {
            if (len >= part.Length && len < bestLength)
            {
                bestLength = len;
            }
        }
        return bestLength;
    }
    
    public static void RawMaterial_Drop(object sender, DragEventArgs e)
    {
        Console.WriteLine("RawMaterial_Drop Executed.");
        
        // the part object being dragged
        var data = e.Data as IDataObject;
        if (data == null)
        {
            Console.WriteLine("Data is null");
            return;
        }

        var part = data.Get("part") as Part;
        var tempPart = data.Get("temp part") as Part;
        var rawMaterialFrom = data.Get("originalRawMaterial") as RawMaterial;
        
        // Get the RawMaterial object from the sender
        var rawMaterialTo = (e.Source as Control)?.Tag as RawMaterial;
        
        // 파트 위에다 드랍하는 경우 이 변수에 그 파트가 저장될 것
        var partTo = (e.Source as Control)?.DataContext as Part;
        
        // 드랍한 위치가 스택패널이 있는 곳, 즉 DragAndDropView인 경우
        if (e.Source is StackPanel)
            Console.WriteLine("DragAndDropView에 드랍함.");
        
        if (rawMaterialFrom == null)
        {
            Console.WriteLine("RawMaterial_Drop - from is null");    
            
            if (rawMaterialTo != null && part != null)
            {
                UpdateRawMaterial(null, rawMaterialTo, part);
                Console.WriteLine("RawMaterial_Drop - from: null, to: not null, part: not null");
            }
            // 우측(TempPartsView)에서 좌측(DragAndDropView)의 빈 공간으로 드랍하는 경우
            else if (e.Source is StackPanel && rawMaterialTo == null && tempPart != null)
            {
                int bestLength = FindBestSizeRawMaterial(GetLengthOptionsRawMaterial(), tempPart);
                RawMaterial newRawMaterial = new RawMaterial(bestLength);
                newRawMaterial.insert_part(tempPart);
                ArrangedRawMaterials.Insert(ArrangedRawMaterials.Count, newRawMaterial);
                
                if (TempPartList.Contains(tempPart))
                {
                    TempPartList.Remove(tempPart);
                }
                // UpdateRawMaterial(rawMaterialFrom, null, tempPartpart);
            }
            // 우측(TempPartsView)에서 좌측(DragAndDropView)의 원자재/파트 위로 드랍하는 경우
            else if ((rawMaterialTo != null || partTo != null) && tempPart != null)
            {
                if (rawMaterialTo != null)  // 원자재 위에다 드랍한 경우
                {
                    rawMaterialTo.insert_part(tempPart);
                    if (TempPartList.Contains(tempPart))
                    {
                        TempPartList.Remove(tempPart);
                    }
                }
                else if (partTo != null)    // 파트 위에다 드랍한 경우
                {
                    // 파트가 속한 원자재 찾기
                    foreach (RawMaterial raw in ArrangedRawMaterials)
                    {
                        foreach (Part p in raw.PartsInside)
                        {
                            if (ReferenceEquals(partTo, p))
                            {
                                rawMaterialTo = raw;
                                break;
                            }
                        }   
                    }

                    if (rawMaterialTo != null)
                    {
                        // 해당 원자재에다 추가
                        rawMaterialTo.insert_part(tempPart);
                        if (TempPartList.Contains(tempPart))
                        {
                            // 이동한 원자재는 원래 있던 곳에서 제거
                            TempPartList.Remove(tempPart);
                        }
                    }
                }
            }
        }
        // rawMaterialFrom이 null이 아닌 경우
        else if (rawMaterialTo != null && part != null)
        {
            // Update the ArrangedRawMaterials collection in the ViewModel
            
            UpdateRawMaterial(rawMaterialFrom, rawMaterialTo, part);
            Console.WriteLine("RawMaterial_Drop - from: not null, to: not null, part: not null");
        }
        else if ((partTo != null || rawMaterialTo == null) && part != null)
        {
            if (e.Source is StackPanel)
            {
                if (rawMaterialTo == null)
                {
                    UpdateRawMaterial(rawMaterialFrom, null, part);
                }
            }
            else
            {
                if (partTo != null)
                {
                    foreach (RawMaterial raw in ArrangedRawMaterials)
                    {
                        foreach (Part p in raw.PartsInside)
                        {
                            if (ReferenceEquals(partTo, p))
                            {
                                rawMaterialTo = raw;
                                break;
                            }
                        }   
                    }
                    Console.WriteLine("여기다 여기" + rawMaterialTo);
                    UpdateRawMaterial(rawMaterialFrom, rawMaterialTo, part);
                }    
            }
            
            
            Console.WriteLine("RawMaterial_Drop - to: null, part: not null");
        }
        

        if (part != null)
        {
            Console.WriteLine(part);
        }
        if (rawMaterialTo == null)
        {
            Console.WriteLine("RawMaterial_Drop - RawMaterial is null");
        }
        
        var partRectangle = data.Get("partRectangle");
        if (partRectangle is Rectangle rect)
            rect.Fill = Brushes.YellowGreen;
        
        // 여기에 RawMaterialSet, TempPartsSet 업데이트 코드 추가 안 해도 되는가??
        MainWindowViewModel.UpdateRawMaterialSet(ArrangedRawMaterials, key);
    }

    public static List<int> GetLengthOptionsRawMaterial()
    {
        return ArrangePartsService.GetLengthOptionsRawMaterial();
    }
}