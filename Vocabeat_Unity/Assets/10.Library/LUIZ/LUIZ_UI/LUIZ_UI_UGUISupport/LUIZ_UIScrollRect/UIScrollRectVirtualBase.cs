using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace LUIZ.UI
{
    /// <summary>
    /// 스크롤 아이템이 매우 많을 경우 이용하는 스크롤. 마스킹 영역 밖은 아이템 생성X 최소한만 생성
    /// 인덱스 기반으로 순환
    /// [ 주의 ] 스크롤 아이템들의 사이즈가 일정해야 하며 대각선 이동은 아직 지원하지 않음
    /// </summary>
    public class UIScrollRectVirtualBase : UIScrollRectSnapJumpBase
    {
        private struct XYData
        {
            public int x;
            public int y;
        }

        private enum ECircularType
        {
            //TODO
            None,
            //BothDir,
            PositiveDir,
            //NegativeDir
        }

        [Header("[ Scroll Virtual ]")]
        [Tooltip("[ 주의 ]정방향 무한 스크롤만 지원. 역방향 무한 스크롤 추가 예정")]
        [SerializeField] private ECircularType CircularType = ECircularType.None;

        //-----------------------------------------------
        private int m_totalItemCount = 0;

        private Vector2 m_itemSize = Vector2.zero;
        private Vector2 m_spacing = Vector2.zero;

        private Vector2Int m_prevTopLeftIndex = Vector2Int.zero;

        private XYData m_gridCount = new XYData(); //스폰된 아이템의 x,y 셀 칸수
        private XYData m_contentCellCount = new XYData(); //전체 content 가로 세로 셀 칸수

        private XYData m_cellSize = new XYData(); //아이템 한개 당 사이즈

        private int m_defaultPaddingTop = 0;
        private int m_defaultPaddingLeft = 0;
        
        private int  m_basePaddingLeft  = 0;
        private int  m_basePaddingTop   = 0;
        private bool m_savedBasePadding = false;
        
        private int m_offsetX = 0;
        private int m_offsetY = 0;

        private bool m_isFixedColumn = true;

        private LayoutGroup m_layoutGroup = null;
        private Vector2 m_lastAnchored;
        
        private List<UITemplateItemBase> m_listItems = null;

        //---------------------------------------------------------
        protected override void OnUIWidgetInitialize(UIFrameBase parentFrame)
        {
            base.OnUIWidgetInitialize(parentFrame);

            if (PrivValidateInitialization() == true)
            {
                m_scrollRect.onValueChanged.AddListener(_ =>
                {
                    if (m_isProgrammaticAdjust) return; //프로그램 이동 중이면 무시
                    PrivCheckScrollMovement(false);
                });
            }
        }
        
        protected override void OnAfterAnchoredPositionChanged()
        {
            var pos = m_scrollRect.content.anchoredPosition;
            if (pos == m_lastAnchored) return;
            m_lastAnchored = pos;
            
            PrivExtractTopLeftIndex(); //내부에서 padding.left,top을 업데이트
        }
        
        protected override void OnAfterReinitPositionSettled()
        {                
            m_scrollRect.StopMovement();
            
            Vector2Int cur = PrivExtractTopLeftIndex();

            PrivResetOffsets();            //순환 버퍼 상태 초기화
            PrivNormalizeSiblingOrder();   //순서 정규화
            PrivRequestScrollItemsAt(cur); //현재 뷰 기준 아이템 즉시 채움

            //이후 onValueChanged에서 비교할 기준 인덱스 동기화
            m_prevTopLeftIndex = cur;
        }
        
        //---------------------------------------------------------
        protected void ProtScrollInfiniteInitialize(int totalItemCount, EInitScrollPosType posOption = EInitScrollPosType.ResetToZero)
        {
            ProtRunReinit(posOption, () =>
            {
                PrivFixContentAnchorsTopLeft();
                PrivFixGridLayoutUpperLeft();
                
                m_totalItemCount = totalItemCount;
                
                m_layoutGroup.padding.left = m_basePaddingLeft;
                m_layoutGroup.padding.top  = m_basePaddingTop;
                m_defaultPaddingLeft = m_basePaddingLeft;
                m_defaultPaddingTop  = m_basePaddingTop;
                
                m_prevTopLeftIndex = Vector2Int.zero;
                m_offsetX = 0;
                m_offsetY = 0;
                
                m_itemSize = PrivGetItemSizeAndSpacing(m_layoutGroup);
                PrivSetGridCellCount(m_itemSize);
                PrivSetContentSize(m_itemSize);
                PrivPrepareScrollItems(); //TODO : 전부 리턴 후 다시 리퀘스트 하는게 아니라 변화된 count 만큼만 리퀘스트 하거나 리턴 하도록 개선
                PrivRequestScrollItems();
            });
        }

        //---------------------------------------------------------
        private bool PrivValidateInitialization()
        {
            m_layoutGroup = m_scrollRect.content.GetComponent<LayoutGroup>();
            if (m_layoutGroup == null)
            {
                Debug.LogError("[UIScrollRectVirtualBase] scroll must have layoutGroup component on content transform");
                return false;
            }
            
            if (!m_savedBasePadding)
            {
                m_basePaddingLeft  = m_layoutGroup.padding.left;
                m_basePaddingTop   = m_layoutGroup.padding.top;
                m_savedBasePadding = true;
            }

            ContentSizeFitter sizeFitter = m_scrollRect.content.GetComponent<ContentSizeFitter>();
            if (sizeFitter != null)
            {
                sizeFitter.enabled = false;
                Debug.LogWarning("[UIScrollRectVirtualBase] ContentSizeFitter disabled because scroll auto scales content size");
            }

            return true;
        }
        
        private void PrivFixContentAnchorsTopLeft()
        {
            var rt = m_scrollRect.content;
            //좌상단 기준
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot     = new Vector2(0f, 1f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        
        private void PrivFixGridLayoutUpperLeft()
        {
            if (m_layoutGroup is GridLayoutGroup gl)
            {
                gl.startCorner   = GridLayoutGroup.Corner.UpperLeft;
                gl.startAxis     = GridLayoutGroup.Axis.Horizontal;
                gl.childAlignment= TextAnchor.UpperLeft;
            }
        }
        
        private void PrivNormalizeSiblingOrder()
        {
            for (int i = 0; i < m_listItems.Count; i++)
                m_listItems[i].transform.SetSiblingIndex(i);
        }
        
        private void PrivResetOffsets()
        {
            m_offsetX = 0;
            m_offsetY = 0;
        }

        private void PrivCheckScrollMovement(bool forceReset)
        {
            Vector2Int indexCurrent = PrivExtractTopLeftIndex();
            Vector2Int indexDifference = m_prevTopLeftIndex - indexCurrent;

            if (forceReset)
            {
                PrivRefreshWidth(indexCurrent, indexDifference.x);
                PrivRefreshHeight(indexCurrent, indexDifference.y);
            }
            else
            {
                if (indexDifference.x != 0)
                {
                    PrivRefreshWidth(indexCurrent, indexDifference.x);
                }

                if (indexDifference.y != 0)
                {
                    PrivRefreshHeight(indexCurrent, indexDifference.y);
                }
            }

            m_prevTopLeftIndex = indexCurrent;
        }

        private void PrivRefreshHeight(Vector2Int indexCurrent, int indexDifferenceY)
        {
            if (indexDifferenceY < 0)//상 -> 하
            {
                indexCurrent.y += (indexDifferenceY + 1);

                for (int y = 0; y < indexDifferenceY * -1; y++)//1프레임에 2개이상 건너뛰는 경우에 대비
                {
                    int indexTopEnd = m_offsetX + (m_offsetY * m_gridCount.x);

                    for (int i = 0; i < m_gridCount.x; i++)
                    {
                        UITemplateItemBase item = m_listItems[indexTopEnd];
                        item.transform.SetSiblingIndex(i + m_gridCount.y * m_gridCount.x);

                        int itemSlotIndex = (i + (indexCurrent.y * m_contentCellCount.x)) + ((m_gridCount.y - 1) * m_contentCellCount.x);

                        PrivCheckItemSlotIndex(itemSlotIndex, item, indexCurrent);
                        indexTopEnd++;
                    }

                    indexCurrent.y++;
                    m_offsetY = (m_offsetY + 1) % m_gridCount.y;
                }
            }
            else//상 <- 하
            {
                indexCurrent.y += (indexDifferenceY - 1);

                for (int y = 0; y < indexDifferenceY; y++)
                {
                    int indexTopBottom = ((m_offsetX + (m_offsetY * m_gridCount.x)) + (m_gridCount.x * (m_gridCount.y - 1))) % (m_gridCount.x * m_gridCount.y);

                    for (int i = 0; i < m_gridCount.x; i++)
                    {
                        UITemplateItemBase item = m_listItems[indexTopBottom];
                        item.transform.SetSiblingIndex(i);

                        int itemSlotIndex = i + (indexCurrent.y * m_contentCellCount.x);

                        PrivCheckItemSlotIndex(itemSlotIndex, item, indexCurrent);
                        indexTopBottom++;
                    }

                    indexCurrent.y--;
                    m_offsetY = (m_offsetY - 1 + m_gridCount.y) % m_gridCount.y;
                }
            }
        }
        
        private void PrivRefreshWidth(Vector2Int indexCurrent, int indexDifferenceX)
        {
            //TODO : 좌 우 스크롤의 경우 정렬 startAxis를 vertical로 가능하도록 해야함, 현재 Horizontal기준으로 계산중
            if(indexDifferenceX < 0)//좌 -> 우
            {
                indexCurrent.x += (indexDifferenceX + 1);

                for (int x = 0; x < indexDifferenceX * -1; x++)//1프레임에 2개이상 건너뛰는 경우에 대비
                {
                    int indexLeftEnd = m_offsetX + (m_offsetY * m_gridCount.x);

                    for (int i = 0; i < m_gridCount.y; i++)
                    {
                        UITemplateItemBase item = m_listItems[indexLeftEnd];
                        item.transform.SetSiblingIndex((i + 1) * m_gridCount.x - 1);

                        int itemSlotIndex = indexCurrent.x + (m_gridCount.x - 1) + (i * m_contentCellCount.x);

                        if(m_isFixedColumn == true)
                            PrivCheckItemSlotIndex(itemSlotIndex, item, indexCurrent);
                        else
                        {
                            if (itemSlotIndex >= m_contentCellCount.x * (i + 1))
                                item.DoUIWidgetHide();
                            else
                                PrivCheckItemSlotIndex(itemSlotIndex, item, indexCurrent);
                        }

                        indexLeftEnd += m_gridCount.x;
                    }
                    indexCurrent.x++;
                    m_offsetX = (m_offsetX + 1) % m_gridCount.x;
                }
            }
            else//좌 <- 우
            {
                indexCurrent.x += (indexDifferenceX - 1);

                for (int x = 0; x < indexDifferenceX; x++)
                {
                    int indexRightEnd = (m_offsetY * m_gridCount.x) 
                        + (m_offsetX + (m_offsetY * m_gridCount.x) + (m_gridCount.x - 1)) 
                        % ((m_offsetY + 1) * m_gridCount.x);

                    for (int i = 0; i < m_gridCount.y; i++)
                    {
                        UITemplateItemBase item = m_listItems[indexRightEnd];
                        item.transform.SetSiblingIndex(i * m_gridCount.x);

                        int itemSlotIndex = indexCurrent.x + (i * m_contentCellCount.x);
                        PrivCheckItemSlotIndex(itemSlotIndex, item, indexCurrent);

                        indexRightEnd += m_gridCount.x;
                    }
                    indexCurrent.x--;
                    m_offsetX = (m_offsetX - 1 + m_gridCount.x) % m_gridCount.x;
                }
            }
        }

        private void PrivCheckItemSlotIndex(int itemSlotIndex, UITemplateItemBase item, Vector2Int indexCurrent)
        {
            if (CircularType == ECircularType.None)
            {
                if (itemSlotIndex >= m_totalItemCount)
                {
                    item.DoUIWidgetHide();
                }
                else
                {
                    item.DoUIWidgetShow();
                    OnUIScrollVirtualRefreshItem(itemSlotIndex, item);
                }
            }
            else
            {
                if (itemSlotIndex >= m_totalItemCount)
                {
                    int rightExceedIndex = itemSlotIndex % m_totalItemCount;
                    OnUIScrollVirtualRefreshItem(rightExceedIndex, item);

                    if (indexCurrent.x == m_contentCellCount.x || indexCurrent.y == m_contentCellCount.y)
                    {
                        DoScrollSnapJumpStop();

                        if (m_scrollRect.IsDragging() == true)
                        {
                            m_scrollRect.enabled = false;
                            m_scrollRect.content.anchoredPosition = Vector2.zero;
                            m_scrollRect.enabled = true;
                        }
                        else
                        {
                            m_scrollRect.content.anchoredPosition = Vector2.zero;
                            InterForceCheckSnap();
                        }
                        PrivExtractTopLeftIndex();
                    }
                }
                else
                {
                    item.DoUIWidgetShow();
                    OnUIScrollVirtualRefreshItem(itemSlotIndex, item);
                }
            }
        }

        private void PrivRequestScrollItemsAt(Vector2Int topLeft)
        {
            int itemIndex = 0;
            int baseSlot  = topLeft.y * m_contentCellCount.x + topLeft.x;

            for (int y = 0; y < m_gridCount.y; y++)
            {
                for (int x = 0; x < m_gridCount.x; x++)
                {
                    if (itemIndex >= m_listItems.Count) break;

                    var item = m_listItems[itemIndex];
                    int slotIndex = baseSlot + x + (y * m_contentCellCount.x);

                    if (CircularType == ECircularType.None)
                    {
                        if (slotIndex >= m_totalItemCount) item.DoUIWidgetHide();
                        else
                        {
                            item.DoUIWidgetShow();
                            OnUIScrollVirtualRefreshItem(slotIndex, item);
                        }
                    }
                    else
                    {
                        if (m_totalItemCount <= 0) item.DoUIWidgetHide();
                        else
                        {
                            int wrapped = slotIndex % m_totalItemCount;
                            item.DoUIWidgetShow();
                            OnUIScrollVirtualRefreshItem(wrapped, item);
                        }
                    }

                    itemIndex++;
                }
            }
        }
        
        private Vector2Int PrivExtractTopLeftIndex()//앵커(0, 1) 현재 뷰포트상 좌상단 아이템의 좌표 인덱스 추출
        {
            Vector2Int topLeftIndex = Vector2Int.zero;
            
            float cellX = (m_itemSize.x + m_spacing.x);
            float cellY = (m_itemSize.y + m_spacing.y);
            
            int rawX = Mathf.FloorToInt((m_scrollRect.content.anchoredPosition.x - m_defaultPaddingLeft) / cellX);
            int rawY = Mathf.FloorToInt((m_scrollRect.content.anchoredPosition.y - m_defaultPaddingTop)  / cellY);

            topLeftIndex.x = Mathf.Max(0, rawX);
            topLeftIndex.y = Mathf.Max(0, rawY);

            int paddingX = topLeftIndex.x * m_cellSize.x + m_defaultPaddingLeft;
            int paddingY = topLeftIndex.y * m_cellSize.y + m_defaultPaddingTop;

            m_layoutGroup.padding.left = paddingX;
            m_layoutGroup.padding.top = paddingY;

            return topLeftIndex;
        }

        private Vector2 PrivGetItemSizeAndSpacing(LayoutGroup layout)
        {
            Vector2 itemSize = Vector2.zero;

            if (layout is GridLayoutGroup gridLayout)
            {
                itemSize = new Vector2(gridLayout.cellSize.x, gridLayout.cellSize.y);
                m_spacing = gridLayout.spacing;
            }
            else if (layout is HorizontalLayoutGroup horizontalLayout)
            {
                itemSize = TemplateItem.GetUISize();
                m_spacing.x = horizontalLayout.spacing;
            }
            else if (layout is VerticalLayoutGroup verticalLayout)
            {
                itemSize = TemplateItem.GetUISize();
                m_spacing.y = verticalLayout.spacing;
            }

            return itemSize;
        }

        private void PrivSetGridCellCount(Vector2 itemSize)
        {
            float vw = m_scrollRect.viewport.rect.width;
            float vh = m_scrollRect.viewport.rect.height;

            float sx = m_spacing.x;
            float sy = m_spacing.y;

            //최소 1 보장
            int cols = m_scrollRect.horizontal ? Mathf.Max(1, Mathf.CeilToInt((vw + sx) / (itemSize.x + sx))) : Mathf.Max(1, Mathf.RoundToInt((vw + sx) / (itemSize.x + sx)));
            int rows = m_scrollRect.vertical ? Mathf.Max(1, Mathf.CeilToInt((vh + sy) / (itemSize.y + sy))) : Mathf.Max(1, Mathf.RoundToInt((vh + sy) / (itemSize.y + sy)));

            m_gridCount.x = cols;
            m_gridCount.y = rows;

            m_cellSize.x = (int)(itemSize.x + m_spacing.x);
            m_cellSize.y = (int)(itemSize.y + m_spacing.y);
        }

        private void PrivSetContentSize(Vector2 itemSize)
        {
            float contentSizeX = 0;
            float contentSizeY = 0;
            int offsetX = 0;
            int offsetY = 0;
            
            m_layoutGroup.padding.left = m_basePaddingLeft;
            m_layoutGroup.padding.top  = m_basePaddingTop;
            m_defaultPaddingLeft = m_basePaddingLeft;
            m_defaultPaddingTop  = m_basePaddingTop;
            
            if (m_scrollRect.vertical && m_scrollRect.horizontal)
            {
                //TODO
/*                if (m_layoutGroup is GridLayoutGroup gridLayout)
                {
                    if (m_totalItemCount % gridLayout.constraintCount > 0)
                        offsetY = 1;

                    m_contentCellCount.y = m_totalItemCount / gridLayout.constraintCount + offsetY;
                    m_contentCellCount.x = gridLayout.constraintCount;

                    contentSizeX = (gridLayout.constraintCount * (itemSize.x + m_spacing.x)) - m_spacing.x;
                    contentSizeY = ((m_totalItemCount / gridLayout.constraintCount) * (itemSize.y + m_spacing.y)) - m_spacing.y;

                    m_gridCount.x = gridLayout.constraintCount;
                    m_gridCount.y += 1;
                    m_isFixedColumn = true;
                }
                else
                {
                    //양쪽 다 사용하려면 GridLayoutGroup이어야함
                }*/
            }
            else
            {
                if (m_scrollRect.horizontal)
                {
                    if (m_totalItemCount % m_gridCount.y > 0)
                        offsetX = 1;
                    
                    m_contentCellCount.x = m_totalItemCount / m_gridCount.y + offsetX;
                    m_contentCellCount.y = m_gridCount.y;

                    contentSizeX = ((m_totalItemCount / m_gridCount.y) * (itemSize.x + m_spacing.x)) - m_spacing.x;
                    contentSizeY = (m_gridCount.y * (itemSize.y + m_spacing.y)) - m_spacing.y;

                    m_gridCount.x += 1; //우측 여유분 1 줄

                    if(m_layoutGroup is GridLayoutGroup)
                        m_isFixedColumn = false;
                    else
                        m_isFixedColumn = true;
                }
                if (m_scrollRect.vertical)
                {
                    if (m_totalItemCount % m_gridCount.x > 0)
                        offsetY = 1;

                    m_contentCellCount.y = m_totalItemCount / m_gridCount.x + offsetY;
                    m_contentCellCount.x = m_gridCount.x;

                    contentSizeX = (m_gridCount.x * (itemSize.x + m_spacing.x)) - m_spacing.x;
                    contentSizeY = ((m_totalItemCount / m_gridCount.x) * (itemSize.y + m_spacing.y)) - m_spacing.y;

                    m_gridCount.y += 1; //아래 여유분 1 줄

                    m_isFixedColumn = true;
                }
            }
            
            contentSizeX += offsetX * (itemSize.x + m_spacing.x) + m_defaultPaddingLeft;
            contentSizeY += offsetY * (itemSize.y + m_spacing.y) + m_defaultPaddingTop;

            if(CircularType != ECircularType.None)
            {
                if (m_scrollRect.horizontal)
                {
                    contentSizeX += ((m_gridCount.x - 1) * (itemSize.x + m_spacing.x));
                }
                if (m_scrollRect.vertical)
                {
                    contentSizeY += ((m_gridCount.y - 1) * (itemSize.y + m_spacing.y));
                }
            }

            m_scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentSizeX);
            m_scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentSizeY);
        }

        private void PrivPrepareScrollItems()
        {
            int needed = m_gridCount.y * m_gridCount.x;

            if (m_listItems == null)
                m_listItems = new List<UITemplateItemBase>(needed);

            int have = m_listItems.Count;

            if (have == needed) return;

            if (have > needed)
            {
                int toReturn = have - needed;
                for (int i = 0; i < toReturn; i++)
                {
                    int last = m_listItems.Count - 1;
                    var item = m_listItems[last];
                    m_listItems.RemoveAt(last);
                    DoUITemplateReturn(item);
                }
            }
            else //have < needed
            {
                int toCreate = needed - have;
                for (int i = 0; i < toCreate; i++)
                {
                    var item = DoTemplateRequestItem(m_scrollRect.content);
                    m_listItems.Add(item);
                }
            }
        }

        private void PrivRequestScrollItems()
        {
            int itemIndex = 0;
            int slotIndex = 0;

            for (int y = 0; y < m_gridCount.y; y++)
            {
                for (int x = 0; x < m_gridCount.x; x++)
                {
                    if (m_listItems.Count <= itemIndex)
                        break;

                    var item = m_listItems[itemIndex];

                    if (CircularType == ECircularType.None)
                    {
                        if (slotIndex >= m_totalItemCount)
                        {
                            item.DoUIWidgetHide();
                        }
                        else
                        {
                            item.DoUIWidgetShow();
                            OnUIScrollVirtualRefreshItem(slotIndex, item);
                        }
                    }
                    else
                    {
                        if (m_totalItemCount <= 0)
                        {
                            item.DoUIWidgetHide();
                        }
                        else
                        {
                            int wrapped = slotIndex % m_totalItemCount;
                            item.DoUIWidgetShow();
                            OnUIScrollVirtualRefreshItem(wrapped, item);
                        }
                    }

                    slotIndex++;
                    itemIndex++;
                }

                slotIndex += (m_contentCellCount.x - m_gridCount.x);
            }
        }

        //-----------------------------------------------------
        protected virtual void OnUIScrollVirtualRefreshItem(int itemIndex, UITemplateItemBase item) { }
    }
}
