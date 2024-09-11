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
    public static ObservableCollection<RawMaterial?> ArrangedRawMaterials { get; set; } = [];
    public static string ArrangementType { get; set; } = "Min Raw Material Type";
    public RawMaterial CurrentRawMaterial { get; set; }
    public static ObservableCollection<Part> PartsCanNotBeArranged { get; set; } = new ObservableCollection<Part>();
    public Part DraggedPart { get; set; }
    private Point _ghostPosition = new(0,0);
    private readonly Point _mouseOffset = new(-5, -5);
    
    public DragAndDropViewModel(ObservableCollection<RawMaterial?> arrangedRawMaterials, ObservableCollection<Part> partsCanNotBeArranged)
    {
        ArrangedRawMaterials = new ObservableCollection<RawMaterial?>(arrangedRawMaterials);
        PartsCanNotBeArranged = new ObservableCollection<Part>(partsCanNotBeArranged);
    }

    public static void UpdateRawMaterial(RawMaterial? from, RawMaterial? to, Part part)
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
                    
                    Console.WriteLine("UpdateRawMaterial - from: not null, to: null, part: not null");
                    List<int> lengthOptions = GetLengthOptionsRawMaterial();
                    int bestLength = FindBestSizeRawMaterial(lengthOptions, part);
            
                    // 새로운 원자재 생성하고 그곳에 파트 추가
                    RawMaterial newRawMaterial = new RawMaterial(bestLength);
                    newRawMaterial.insert_part(part);
                    ArrangedRawMaterials.Insert(ArrangedRawMaterials.Count, newRawMaterial);
                    
                    // 원래 파트가 있던 원자재에서 파트를 제거.
                    from.removePart(part);
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
                    // 여기에 원자재 길이 조정하는 코드~
                    // 가능한 길이 어데서 갖고오느냐?
                    // 바로
                    int bestLengthOption = to.findPossibleRawLengthToIncrease(part);
                    
                    Console.WriteLine("==========================================");
                    Console.WriteLine(to.isAddingPossible(part));
                    Console.WriteLine(bestLengthOption);
                    Console.WriteLine("==========================================");

                    
                    if (!to.isAddingPossible(part) && bestLengthOption != -1)
                    {
                        to.increaseRawLength(bestLengthOption, part);
                    }
                    
                    // --------------------------------------------------------------------------------
                    to.insert_part(part);
                    from.removePart(part);
                    if (from.PartsInside.Count == 0)
                    {
                        ArrangedRawMaterials.Remove(from);
                    }
                    Console.WriteLine("UpdateRawMaterial - from: not null, to: not null, part: not null");
                }
            }
        }
        
        MainWindowViewModel.UpdateRawMaterialSet(ArrangedRawMaterials);
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
    
    public static void RawMaterial_Drop(object? sender, DragEventArgs e)
    {
        try
        {
            DragAndDropView.InitializeSortOption();
        
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
            var borderTo = (e.Source as Border);
            if (borderTo != null)
            {
                Console.WriteLine("보더에 드랍합!");
                partTo = borderTo.Child?.DataContext as Part;
                Console.WriteLine("파트의 정체는? " + partTo);
            }
        
            // 이 경우는 우측(TempPartsView)에서 좌측(DragAndDropView)로 드랍하는 경우일 것
            if (rawMaterialFrom == null)
            {
                Console.WriteLine("RawMaterial_Drop - from is null");    
                
                if (rawMaterialTo != null && part != null)
                {
                    UpdateRawMaterial(null, rawMaterialTo, part);
                    Console.WriteLine("RawMaterial_Drop - from: null, to: not null, part: not null");
                }
                // 우측(TempPartsView)에서 좌측(DragAndDropView)의 빈 공간으로 드랍하는 경우
                else if (e.Source is not DockPanel && rawMaterialTo == null && tempPart != null && partTo == null)
                {
                    int bestLength = FindBestSizeRawMaterial(GetLengthOptionsRawMaterial(), tempPart);
                    RawMaterial newRawMaterial = new RawMaterial(bestLength);
                    newRawMaterial.insert_part(tempPart);
                    ArrangedRawMaterials.Insert(ArrangedRawMaterials.Count, newRawMaterial);
                
                    if (PartsCanNotBeArranged.Contains(tempPart))
                    {
                        PartsCanNotBeArranged.Remove(tempPart);
                    }
                    // UpdateRawMaterial(rawMaterialFrom, null, tempPartpart);
                }
                // 우측(TempPartsView)에서 좌측(DragAndDropView)의 원자재/파트 위로 드랍하는 경우
                else if ((rawMaterialTo != null || partTo != null) && tempPart != null)
                {
                    if (rawMaterialTo != null)  // 원자재 위에다 드랍한 경우
                    {
                        // 여기도 원자재 길이 변경하는 코드~
                        int bestLengthOption = rawMaterialTo.findPossibleRawLengthToIncrease(tempPart);
                        if (!rawMaterialTo.isAddingPossible(tempPart) && bestLengthOption != -1)
                        {
                            rawMaterialTo.increaseRawLength(bestLengthOption, tempPart);
                        }
                    
                    
                        rawMaterialTo.insert_part(tempPart);
                        if (PartsCanNotBeArranged.Contains(tempPart))
                        {
                            PartsCanNotBeArranged.Remove(tempPart);
                        }
                    }
                    else if (partTo != null)    // 파트 위에다 드랍한 경우
                    {
                        // 파트가 속한 원자재 찾기
                        foreach (RawMaterial? raw in ArrangedRawMaterials)
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
                            // 여기도 원자재 길이 변경하는 코드 추가~
                            int bestLengthOption = rawMaterialTo.findPossibleRawLengthToIncrease(tempPart);
                            Console.WriteLine("==========================================");
                            Console.WriteLine(rawMaterialTo.isAddingPossible(tempPart));
                            Console.WriteLine(bestLengthOption);
                            Console.WriteLine("==========================================");
                        
                            if (!rawMaterialTo.isAddingPossible(tempPart) && bestLengthOption != -1)
                            {
                                rawMaterialTo.increaseRawLength(bestLengthOption, tempPart);
                            }
                        
                        
                            rawMaterialTo.insert_part(tempPart);
                            if (PartsCanNotBeArranged.Contains(tempPart))
                            {
                                // 이동한 원자재는 원래 있던 곳에서 제거
                                PartsCanNotBeArranged.Remove(tempPart);
                            }
                        }
                    }
                }
            }
            // rawMaterialFrom이 null이 아닌 경우
            else if (rawMaterialTo != null && part != null)
            {
                // Update the ArrangedRawMaterials collection in the ViewModel
                Console.WriteLine("여기일 걸?");
                UpdateRawMaterial(rawMaterialFrom, rawMaterialTo, part);
                Console.WriteLine("RawMaterial_Drop - from: not null, to: not null, part: not null");
            }
            else if ((partTo != null || rawMaterialTo == null) && part != null)
            {
                // if (e.Source is StackPanel || e.Source is TextBlock)
                if (e.Source is not DockPanel && partTo == null)
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
                        foreach (RawMaterial? raw in ArrangedRawMaterials)
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
                        // ================================================
                        var control = e.Source as Control;
                        if (control == null) return;
                        var dropPosition = e.GetPosition(control);
                        var controlBounds = control.Bounds;

                        double midpointX = controlBounds.Left + (controlBounds.Width / 2);
                        
                        if (dropPosition.X < midpointX)
                            Console.WriteLine("왼쪽에 드랍");
                        else
                            Console.WriteLine("오른쪽에 드랍");
                        
                        // ================================================
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
        
            // 여기에 RawMaterialSet, TempPartsSet 업데이트 코드 추가 안 해도 되는가??
            MainWindowViewModel.UpdateRawMaterialSet(ArrangedRawMaterials);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }

    public static List<int> GetLengthOptionsRawMaterial()
    {
        return ArrangePartsService.GetLengthOptionsRawMaterial();
    }
}