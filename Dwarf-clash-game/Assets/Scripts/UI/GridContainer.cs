//using UnityEngine;
//using UnityEngine.UIElements;
//using System.Xml.Serialization;
//using System;


//#if UNITY_EDITOR
//using UnityEditor;
//#endif

///// <summary>
///// Кастомный VisualElement, который создает сетку дочерних элементов
///// с динамическим расчетом их размера и возможностью использовать UXML-шаблон для ячеек.
///// </summary>
//public class GridContainer : VisualElement
//{
//    #region --- Backing Fields and Properties ---

//    // Приватные поля для хранения данных
//    private int m_WidthCount = 1;
//    private int m_HeightCount = 1;
//    private int m_Spacing = 0;
//    private VisualTreeAsset m_CellTemplateAsset;
//    public Type myType { get; set; }

//#if UNITY_EDITOR
//    // Это поле используется для связи с UXML атрибутом
//    private string m_CellTemplatePath; 
//    #endif

//    #endregion

//    #region --- Public Properties for Inspector ---

//    // Публичное свойство типа Object, которое видит Инспектор UI Builder.
//    public Object cellTemplate
//    {
//        get => m_CellTemplateAsset;
//        set
//        {
//            if (value == m_CellTemplateAsset)
//                return;

//            if (value is VisualTreeAsset vta)
//            {
//                m_CellTemplateAsset = vta;
//                #if UNITY_EDITOR
//                // Когда пользователь перетаскивает ассет в инспектор,
//                // мы получаем его путь и обновляем строковый атрибут в UXML.
//                m_CellTemplatePath = AssetDatabase.GetAssetPath(m_CellTemplateAsset);
//                m_CellTemplatePathAttr.SetValueToBag(this, m_CellTemplatePath);
//                #endif
//            }
//            else
//            {
//                m_CellTemplateAsset = null;
//                #if UNITY_EDITOR
//                m_CellTemplatePath = null;
//                m_CellTemplatePathAttr.SetValueToBag(this, string.Empty);
//                #endif
//            }
            
//            GenerateAndResizeCells();
//        }
//    }

//    // Остальные публичные свойства для инспектора
//    public int widthCount
//    {
//        get => m_WidthCount;
//        set
//        {
//            if (m_WidthCount == value) return;
//            m_WidthCount = value > 0 ? value : 1;
//            GenerateAndResizeCells();
//        }
//    }
//    public int heightCount
//    {
//        get => m_HeightCount;
//        set
//        {
//            if (m_HeightCount == value) return;
//            m_HeightCount = value > 0 ? value : 1;
//            GenerateAndResizeCells();
//        }
//    }
//    public int spacing
//    {
//        get => m_Spacing;
//        set
//        {
//            if (m_Spacing == value) return;
//            m_Spacing = value;
//            RecalculateCellSizes();
//        }
//    }

//    #endregion

//    #region --- UXML Factory & Traits ---

//    public new class UxmlFactory : UxmlFactory<GridContainer, UxmlTraits> { }

//    public new class UxmlTraits : VisualElement.UxmlTraits
//    {
//        // Атрибуты для UXML, которые будут видны в инспекторе.
//        private UxmlIntAttributeDescription m_WidthCountAttr = new UxmlIntAttributeDescription { name = "width-count", defaultValue = 1 };
//        private UxmlIntAttributeDescription m_HeightCountAttr = new UxmlIntAttributeDescription { name = "height-count", defaultValue = 1 };
//        private UxmlIntAttributeDescription m_SpacingAttr = new UxmlIntAttributeDescription { name = "spacing", defaultValue = 0 };
//        // Используем СТРОКУ для хранения пути к шаблону в UXML.
//        private readonly UxmlStringAttributeDescription m_CellTemplatePathAttr = new UxmlStringAttributeDescription { name = "cell-template-path", defaultValue = "" };
        
//        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
//        {
//            base.Init(ve, bag, cc);
//            var grid = ve as GridContainer;
            
//            grid.widthCount = m_WidthCountAttr.GetValueFromBag(bag, cc);
//            grid.heightCount = m_HeightCountAttr.GetValueFromBag(bag, cc);
//            grid.spacing = m_SpacingAttr.GetValueFromBag(bag, cc);

//            // Сохраняем ссылку на описание атрибута, чтобы свойство cellTemplate могло его изменять
//            grid.m_CellTemplatePathAttr = m_CellTemplatePathAttr;

//#if UNITY_EDITOR
//            // При загрузке UXML, читаем путь и загружаем ассет
//            string templatePath = m_CellTemplatePathAttr.GetValueFromBag(bag, cc);
//            if (!string.IsNullOrEmpty(templatePath))
//            {
//                grid.cellTemplate = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);
//            }
//#endif
//            grid.GenerateAndResizeCells();
//        }
//    }
    
//    private UxmlStringAttributeDescription m_CellTemplatePathAttr;

//    #endregion

//    #region --- Logic ---

//    public GridContainer()
//    {
//        style.flexGrow = 1;
//        style.flexDirection = FlexDirection.Row;
//        style.flexWrap = Wrap.Wrap;
//        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
//    }

//    private void GenerateAndResizeCells()
//    {
//        GenerateCells();
//        RecalculateCellSizes();
//    }
    
//    private void GenerateCells()
//    {
//        Clear(); 
//        int totalCells = widthCount * heightCount;
//        for (int i = 0; i < totalCells; i++)
//        {
//            VisualElement cell;
//            if (m_CellTemplateAsset != null)
//            {
//                cell = m_CellTemplateAsset.Instantiate();
//            }
//            else
//            {
//                cell = new VisualElement();
//                cell.style.borderTopWidth = 1;
//                cell.style.borderRightWidth = 1;
//                cell.style.borderBottomWidth = 1;
//                cell.style.borderLeftWidth = 1;
//                cell.style.borderTopColor = Color.gray;
//                cell.style.borderRightColor = Color.gray;
//                cell.style.borderBottomColor = Color.gray;
//                cell.style.borderLeftColor = Color.gray;
//            }
//            cell.name = $"cell-{i}";
//            Add(cell);
//        }
//    }
    
//    private void OnGeometryChanged(GeometryChangedEvent evt)
//    {
//        RecalculateCellSizes();
//    }
    
//    private void RecalculateCellSizes()
//    {
//        if (float.IsNaN(resolvedStyle.width) || resolvedStyle.width <= 0f)
//            return;
            
//        float totalHorizontalSpacing = spacing * (widthCount - 1);
//        float totalVerticalSpacing = spacing * (heightCount - 1);

//        float cellWidth = (resolvedStyle.width - totalHorizontalSpacing) / widthCount;
//        float cellHeight = (resolvedStyle.height - totalVerticalSpacing) / heightCount;
        
//        if (cellWidth <= 0 || cellHeight <= 0)
//            return;

//        for (int i = 0; i < childCount; i++)
//        {
//            var cell = this[i];
//            cell.style.width = cellWidth;
//            cell.style.height = cellHeight;
//            bool isLastInRow = (i + 1) % widthCount == 0;
//            cell.style.marginRight = isLastInRow ? 0 : spacing;
//            bool isLastRow = i >= (heightCount - 1) * widthCount;
//            cell.style.marginBottom = isLastRow ? 0 : spacing;
//        }
//    }
//    #endregion
//}
