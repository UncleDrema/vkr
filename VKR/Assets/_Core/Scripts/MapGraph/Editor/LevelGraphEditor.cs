using UnityEditor;
using UnityEngine;

namespace Game.MapGraph.Editor
{
    [CustomEditor(typeof(LevelGraph))]
    public class LevelGraphEditor : UnityEditor.Editor
    {
        private LevelGraph graph;
        private int firstNodeIndex = -1;

        private void OnEnable()
        {
            graph = (LevelGraph)target;
            Tools.current = Tool.None;
        }

        private void OnDisable()
        {
            Tools.current = Tool.Move;
        }

        private void OnSceneGUI()
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Event e = Event.current;
            Transform t = graph.transform;

            // 1. Рисуем ребра
            Handles.color = graph.edgeColor;
            foreach (var edge in graph.edges)
            {
                if (edge.a >= 0 && edge.a < graph.nodes.Count &&
                    edge.b >= 0 && edge.b < graph.nodes.Count)
                {
                    Vector3 pa = t.TransformPoint(graph.nodes[edge.a]);
                    Vector3 pb = t.TransformPoint(graph.nodes[edge.b]);
                    Handles.DrawLine(pa, pb);
                }
            }

            // 2. Рисуем и двигаем узлы
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                Vector3 worldPos = t.TransformPoint(graph.nodes[i]);
                float size = HandleUtility.GetHandleSize(worldPos) * graph.handleSize;
                Handles.color = (i == firstNodeIndex) ? graph.selectedColor : graph.nodeColor;

                EditorGUI.BeginChangeCheck();
                Vector3 newWorld = Handles.FreeMoveHandle(
                    worldPos,
                    size,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(graph, "Move Graph Node");
                    var local = t.InverseTransformPoint(newWorld);
                    local.y = 0;
                    graph.nodes[i] = local;
                    EditorUtility.SetDirty(graph);
                }
            }

            // 3. ЛКМ по пустому месту — добавить узел
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt && firstNodeIndex < 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out var hit))
                {
                    Undo.RecordObject(graph, "Add Graph Node");
                    var local = t.InverseTransformPoint(hit.point);
                    local.y = 0;
                    graph.nodes.Add(local);
                    EditorUtility.SetDirty(graph);
                    e.Use();
                }
            }

            // 4. ЛКМ по узлу — выбираем его для создания ребра
            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    Vector3 worldPos = t.TransformPoint(graph.nodes[i]);
                    float size = HandleUtility.GetHandleSize(worldPos) * graph.handleSize * 1.2f;
                    Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPos);
                    if ((guiPos - e.mousePosition).sqrMagnitude < size * size)
                    {
                        e.Use();
                        if (firstNodeIndex < 0)
                        {
                            firstNodeIndex = i;
                        }
                        else if (firstNodeIndex != i)
                        {
                            // добавляем ребро между firstNodeIndex и i, если его нет
                            bool exists = graph.edges.Exists(ed =>
                                (ed.a == firstNodeIndex && ed.b == i) ||
                                (ed.a == i && ed.b == firstNodeIndex));
                            if (!exists)
                            {
                                Undo.RecordObject(graph, "Add Graph Edge");
                                graph.edges.Add(new LevelGraph.Edge { a = firstNodeIndex, b = i });
                                EditorUtility.SetDirty(graph);
                            }

                            firstNodeIndex = -1;
                        }

                        break;
                    }
                }
            }

            // 5. RMB по узлу — удаление узла и всех его ребер
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                for (int i = 0; i < graph.nodes.Count; i++)
                {
                    Vector3 worldPos = t.TransformPoint(graph.nodes[i]);
                    float size = HandleUtility.GetHandleSize(worldPos) * graph.handleSize * 1.2f;
                    Vector2 guiPos = HandleUtility.WorldToGUIPoint(worldPos);
                    if ((guiPos - e.mousePosition).sqrMagnitude < size * size)
                    {
                        Undo.RecordObject(graph, "Remove Graph Node");
                        // удаляем все ребра, связанные с i
                        graph.edges.RemoveAll(ed => ed.a == i || ed.b == i);
                        // сдвигаем индексы в ребрах
                        for (int j = 0; j < graph.edges.Count; j++)
                        {
                            var ed = graph.edges[j];
                            if (ed.a > i) ed.a--;
                            if (ed.b > i) ed.b--;
                            graph.edges[j] = ed;
                        }

                        graph.nodes.RemoveAt(i);
                        firstNodeIndex = -1;
                        EditorUtility.SetDirty(graph);
                        e.Use();
                        break;
                    }
                }
            }
        }
    }
}