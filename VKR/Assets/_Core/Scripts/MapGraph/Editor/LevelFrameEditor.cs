using UnityEditor;
using UnityEngine;

namespace Game.MapGraph.Editor
{
    [CustomEditor(typeof(LevelFrame))]
    public class LevelFrameEditor : UnityEditor.Editor
    {
        private LevelFrame frame;
        private int selectedIndex = -1;

        private void OnEnable()
        {
            frame = (LevelFrame)target;
            Tools.current = Tool.None;
        }

        private void OnDisable()
        {
            Tools.current = Tool.Move;
        }

        // Отрисовка gizmos в режиме редактирования
        private void OnSceneGUI()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            
            Event e = Event.current;
            Transform t = frame.transform;

            Handles.color = frame.lineColor;
            // Рисуем полигон
            for (int i = 0; i < frame.points.Count; i++)
            {
                Vector3 p0 = t.TransformPoint(frame.points[i]);
                Vector3 p1 = t.TransformPoint(frame.points[(i + 1) % frame.points.Count]);
                Handles.DrawLine(p0, p1);
            }

            // Рисуем и редактируем точки
            Handles.color = frame.pointColor;
            for (int i = 0; i < frame.points.Count; i++)
            {
                Vector3 worldPos = t.TransformPoint(frame.points[i]);
                float size = HandleUtility.GetHandleSize(worldPos) * frame.handleSize;

                // свободно двигать точку по XZ-плоскости
                Vector3 newWorldPos = Handles.FreeMoveHandle(
                    worldPos,
                    size,
                    Vector3.zero,
                    Handles.CubeHandleCap
                );

                if (worldPos != newWorldPos)
                {
                    Undo.RecordObject(frame, "Move Frame Point");
                    Vector3 localPos = t.InverseTransformPoint(newWorldPos);
                    localPos.y = 0;
                    frame.points[i] = localPos;
                }
            }

            // Клик правой кнопкой на ребре — добавить вершину
            if (e.type == EventType.MouseDown && e.button == 1 && !e.alt)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Vector3 clickPoint = hit.point;
                    // Ищем ближайшее ребро
                    float minDist = float.MaxValue;
                    int insertIdx = -1;
                    for (int i = 0; i < frame.points.Count; i++)
                    {
                        Vector3 a = t.TransformPoint(frame.points[i]);
                        Vector3 b = t.TransformPoint(frame.points[(i + 1) % frame.points.Count]);
                        float d = HandleUtility.DistancePointToLineSegment(clickPoint, a, b);
                        if (d < minDist && d < 1.0f) // порог 1м
                        {
                            minDist = d;
                            insertIdx = i + 1;
                        }
                    }

                    if (insertIdx >= 0)
                    {
                        Undo.RecordObject(frame, "Insert Frame Point");
                        Vector3 localNew = t.InverseTransformPoint(clickPoint);
                        localNew.y = 0;
                        frame.points.Insert(insertIdx, localNew);
                        e.Use();
                    }
                }
            }

            // Клик средней кнопкой на вершине — удалить
            if (e.type == EventType.MouseDown && e.button == 2)
            {
                for (int i = 0; i < frame.points.Count; i++)
                {
                    Vector3 worldPos = t.TransformPoint(frame.points[i]);
                    float size = HandleUtility.GetHandleSize(worldPos) * frame.handleSize;
                    if ((HandleUtility.WorldToGUIPoint(worldPos) - e.mousePosition).sqrMagnitude < (size * 20f))
                    {
                        if (frame.points.Count > 3)
                        {
                            Undo.RecordObject(frame, "Remove Frame Point");
                            frame.points.RemoveAt(i);
                        }

                        e.Use();
                        break;
                    }
                }
            }

            // Обновляем инспектор
            if (GUI.changed)
                EditorUtility.SetDirty(frame);
        }
    }
}