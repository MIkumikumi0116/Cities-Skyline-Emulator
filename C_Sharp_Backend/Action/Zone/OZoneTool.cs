using ColossalFramework.Math;
using ColossalFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace C_Sharp_Backend.Action.Zone
{
    public class OZoneTool : ZoneTool
    {
        private struct FillPos
        {
            public byte m_x;

            public byte m_z;
        }

        public Vector3 m_startPosition;

        public Vector3 m_startDirection;

        public Vector3 m_mousePosition;

        private Vector3 m_mouseDirection;

        private Ray m_mouseRay;

        private float m_mouseRayLength;

        private Vector3 m_cameraDirection;

        public bool m_zoning;

        private bool m_dezoning;

        private bool m_validPosition;

        private ushort[] m_closeSegments;

        private int m_closeSegmentCount;

        private ulong[] m_fillBuffer1;

        private ulong[] m_fillBuffer2;

        private ulong[] m_fillBuffer3;

        private FastList<FillPos> m_fillPositions;

        private bool m_mouseRayValid;

        private object m_dataLock;

        protected override void Awake()
        {
            base.Awake();
            m_closeSegments = new ushort[16];
            m_fillBuffer1 = new ulong[64];
            m_fillBuffer2 = new ulong[64];
            m_fillBuffer3 = new ulong[64];
            m_fillPositions = new FastList<FillPos>();
            m_dataLock = new object();

            // 找到ToolController
            ToolController toolController = GameObject.FindObjectOfType<ToolController>();
            m_toolController = toolController;
        }

        protected override void OnToolGUI(Event e)
        {
            bool isInsideUI = m_toolController.IsInsideUI;
            if (e.type == EventType.MouseDown)
            {
                if (!isInsideUI)
                {
                    if (e.button == 0)
                    {
                        Singleton<SimulationManager>.instance.AddAction(BeginZoning());
                    }
                    else if (e.button == 1)
                    {
                        Singleton<SimulationManager>.instance.AddAction(BeginDezoning());
                    }
                }
            }
            else if (e.type == EventType.MouseUp)
            {
                if (e.button == 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(EndZoning(!isInsideUI));
                }
                else if (e.button == 1)
                {
                    Singleton<SimulationManager>.instance.AddAction(EndDezoning(!isInsideUI));
                }
            }
        }

        public void Test()
        {

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Singleton<TerrainManager>.instance.RenderZones = true;
            Shader.SetGlobalFloat("_ForceZoneColors", 1f);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            base.ToolCursor = null;
            Shader.SetGlobalFloat("_ForceZoneColors", 0f);
            Singleton<TerrainManager>.instance.RenderZones = false;
            m_validPosition = false;
            m_zoning = false;
            m_dezoning = false;
            m_mouseRayValid = false;
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }

            bool zoning;
            bool dezoning;
            bool validPosition;
            Vector3 startPosition;
            Vector3 mousePosition;
            Vector3 startDirection;
            Vector3 mouseDirection;
            try
            {
                zoning = m_zoning;
                dezoning = m_dezoning;
                validPosition = m_validPosition;
                startPosition = m_startPosition;
                mousePosition = m_mousePosition;
                startDirection = m_startDirection;
                mouseDirection = m_mouseDirection;
                for (int i = 0; i < 64; i++)
                {
                    m_fillBuffer3[i] = m_fillBuffer2[i];
                }
            }
            finally
            {
                Monitor.Exit(m_dataLock);
            }

            if ((!zoning && !dezoning && !validPosition) || !Cursor.visible || m_toolController.IsInsideUI)
            {
                base.RenderOverlay(cameraInfo);
                return;
            }

            Color color = ((!zoning && !dezoning) ? ((m_zone > ItemClass.Zone.Unzoned && !dezoning) ? Singleton<ZoneManager>.instance.m_properties.m_zoneColors[(int)m_zone] : Singleton<ZoneManager>.instance.m_properties.m_unzoneColor) : Singleton<ZoneManager>.instance.m_properties.m_activeColor);
            switch (m_mode)
            {
                case Mode.Select:
                    {
                        Vector3 vector5 = ((!zoning && !dezoning) ? mousePosition : startPosition);
                        Vector3 vector6 = mousePosition;
                        Vector3 vector7 = ((!zoning && !dezoning) ? mouseDirection : startDirection);
                        Vector3 vector8 = new Vector3(vector7.z, 0f, 0f - vector7.x);
                        float num10 = Mathf.Round(((vector6.x - vector5.x) * vector7.x + (vector6.z - vector5.z) * vector7.z) * 0.125f) * 8f;
                        float num11 = Mathf.Round(((vector6.x - vector5.x) * vector8.x + (vector6.z - vector5.z) * vector8.z) * 0.125f) * 8f;
                        float num12 = ((!(num10 >= 0f)) ? (-4f) : 4f);
                        float num13 = ((!(num11 >= 0f)) ? (-4f) : 4f);
                        Quad3 quad2 = default(Quad3);
                        quad2.a = vector5 - vector7 * num12 - vector8 * num13;
                        quad2.b = vector5 - vector7 * num12 + vector8 * (num11 + num13);
                        quad2.c = vector5 + vector7 * (num10 + num12) + vector8 * (num11 + num13);
                        quad2.d = vector5 + vector7 * (num10 + num12) - vector8 * num13;
                        if (num12 != num13)
                        {
                            Vector3 b2 = quad2.b;
                            quad2.b = quad2.d;
                            quad2.d = b2;
                        }

                        Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
                        Singleton<RenderManager>.instance.OverlayEffect.DrawQuad(cameraInfo, color, quad2, -1f, 1025f, renderLimits: false, alphaBlend: true);
                        break;
                    }
                case Mode.Brush:
                    Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
                    Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(cameraInfo, color, mousePosition, m_brushSize, -1f, 1025f, renderLimits: false, alphaBlend: true);
                    break;
                case Mode.Fill:
                    {
                        Vector3 vector = mouseDirection;
                        Vector3 vector2 = new Vector3(vector.z, 0f, 0f - vector.x);
                        int num = -1;
                        ulong num2 = m_fillBuffer3[0];
                        for (int j = 0; j <= 64; j++)
                        {
                            int num3 = -1;
                            if (j == 64 || m_fillBuffer3[j] != num2)
                            {
                                if (num != -1)
                                {
                                    int num4 = j - 1;
                                    for (int k = 0; k <= 64; k++)
                                    {
                                        if (k == 64 || (num2 & (ulong)(1L << k)) == 0)
                                        {
                                            if (num3 != -1)
                                            {
                                                int num5 = k - 1;
                                                Vector3 vector3 = mousePosition + vector * ((num3 - 32) * 8) + vector2 * ((num - 32) * 8);
                                                Vector3 vector4 = mousePosition + vector * ((num5 - 32) * 8) + vector2 * ((num4 - 32) * 8);
                                                float num6 = Mathf.Round(((vector4.x - vector3.x) * vector.x + (vector4.z - vector3.z) * vector.z) * 0.125f) * 8f;
                                                float num7 = Mathf.Round(((vector4.x - vector3.x) * vector2.x + (vector4.z - vector3.z) * vector2.z) * 0.125f) * 8f;
                                                float num8 = ((!(num6 >= 0f)) ? (-4f) : 4f);
                                                float num9 = ((!(num7 >= 0f)) ? (-4f) : 4f);
                                                Quad3 quad = default(Quad3);
                                                quad.a = vector3 - vector * num8 - vector2 * num9;
                                                quad.b = vector3 - vector * num8 + vector2 * (num7 + num9);
                                                quad.c = vector3 + vector * (num6 + num8) + vector2 * (num7 + num9);
                                                quad.d = vector3 + vector * (num6 + num8) - vector2 * num9;
                                                if (num8 != num9)
                                                {
                                                    Vector3 b = quad.b;
                                                    quad.b = quad.d;
                                                    quad.d = b;
                                                }

                                                Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
                                                Singleton<RenderManager>.instance.OverlayEffect.DrawQuad(cameraInfo, color, quad, -1f, 1025f, renderLimits: false, alphaBlend: false);
                                            }

                                            num3 = -1;
                                        }
                                        else if (num3 == -1)
                                        {
                                            num3 = k;
                                        }
                                    }
                                }

                                if (j != 64 && m_fillBuffer3[j] != 0)
                                {
                                    num = j;
                                    num2 = m_fillBuffer3[j];
                                }
                                else
                                {
                                    num = -1;
                                }
                            }
                            else if (num == -1 && m_fillBuffer3[j] != 0)
                            {
                                num = j;
                                num2 = m_fillBuffer3[j];
                            }
                        }

                        break;
                    }
            }

            base.RenderOverlay(cameraInfo);
        }

        protected override void OnToolUpdate()
        {
            ItemClass.Zone zone = ((m_zone > ItemClass.Zone.Unzoned && !m_dezoning) ? m_zone : ItemClass.Zone.Unzoned);
            if (m_zoneCursors != null && m_zoneCursors.Length > (int)zone)
            {
                base.ToolCursor = m_zoneCursors[(int)zone];
            }
        }

        protected override void OnToolLateUpdate()
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 cameraDirection = Vector3.Cross(Camera.main.transform.right, Vector3.up);
            cameraDirection.Normalize();
            while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }

            try
            {
                m_mouseRay = Camera.main.ScreenPointToRay(mousePosition);
                m_mouseRayLength = Camera.main.farClipPlane;
                m_cameraDirection = cameraDirection;
                m_mouseRayValid = !m_toolController.IsInsideUI && Cursor.visible;
            }
            finally
            {
                Monitor.Exit(m_dataLock);
            }

            ToolBase.OverrideInfoMode = false;
        }

        private IEnumerator BeginZoning()
        {
            if (m_validPosition)
            {
                while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                {
                }

                try
                {
                    m_dezoning = false;
                    if (!m_zoning)
                    {
                        m_zoning = true;
                        m_startPosition = m_mousePosition;
                        m_startDirection = m_mouseDirection;
                    }
                }
                finally
                {
                    Monitor.Exit(m_dataLock);
                }
            }

            yield return 0;
        }

        private IEnumerator BeginDezoning()
        {
            if (m_validPosition)
            {
                while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                {
                }

                try
                {
                    m_zoning = false;
                    if (!m_dezoning)
                    {
                        m_dezoning = true;
                        m_startPosition = m_mousePosition;
                        m_startDirection = m_mouseDirection;
                    }
                }
                finally
                {
                    Monitor.Exit(m_dataLock);
                }
            }

            yield return 0;
        }

        private IEnumerator EndZoning(bool applyChanges)
        {
            if (m_zoning)
            {
                if (applyChanges)
                {
                    if (m_mode == Mode.Select)
                    {
                        ApplyZoning();
                    }

                    if (m_mode == Mode.Fill)
                    {
                        ApplyFill();
                    }
                }

                m_zoning = false;
            }

            yield return 0;
        }

        private IEnumerator EndDezoning(bool applyChanges)
        {
            if (m_dezoning)
            {
                if (applyChanges)
                {
                    if (m_mode == Mode.Select)
                    {
                        ApplyZoning();
                    }

                    if (m_mode == Mode.Fill)
                    {
                        ApplyFill();
                    }
                }

                m_dezoning = false;
            }

            yield return 0;
        }

        private void Snap(ref Vector3 point, ref Vector3 direction, ref ItemClass.Zone zone, ref bool occupied1, ref bool occupied2, ref ZoneBlock block)
        {
            direction = new Vector3(Mathf.Cos(block.m_angle), 0f, Mathf.Sin(block.m_angle));
            Vector3 vector = direction * 8f;
            Vector3 vector2 = new Vector3(vector.z, 0f, 0f - vector.x);
            Vector3 vector3 = block.m_position + vector * 0.5f + vector2 * 0.5f;
            Vector2 vector4 = new Vector2(point.x - vector3.x, point.z - vector3.z);
            int num = Mathf.RoundToInt((vector4.x * vector.x + vector4.y * vector.z) * (1f / 64f));
            int num2 = Mathf.RoundToInt((vector4.x * vector2.x + vector4.y * vector2.z) * (1f / 64f));
            point.x = vector3.x + (float)num * vector.x + (float)num2 * vector2.x;
            point.z = vector3.z + (float)num * vector.z + (float)num2 * vector2.z;
            if (num >= -4 && num < 0 && num2 >= -4 && num2 < 4)
            {
                zone = block.GetZone(num + 4, num2 + 4);
                occupied1 = block.IsOccupied1(num + 4, num2 + 4);
                occupied2 = block.IsOccupied2(num + 4, num2 + 4);
            }
        }

        public override void SimulationStep()
        {
            while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
            {
            }

            Debug.Log($"OZoneTool->SimulationStep");
            Ray mouseRay;
            Vector3 cameraDirection;
            bool mouseRayValid;
            try
            {
                mouseRay = m_mouseRay;
                cameraDirection = m_cameraDirection;
                mouseRayValid = m_mouseRayValid;
            }
            finally
            {
                Monitor.Exit(m_dataLock);
            }

            if (m_mode == Mode.Fill)
            {
                GuideController properties = Singleton<GuideManager>.instance.m_properties;
                if ((object)properties != null)
                {
                    Singleton<ZoneManager>.instance.m_optionsNotUsed.Activate(properties.m_zoneOptionsNotUsed);
                }
            }
            else
            {
                GenericGuide optionsNotUsed = Singleton<ZoneManager>.instance.m_optionsNotUsed;
                if (optionsNotUsed != null && !optionsNotUsed.m_disabled)
                {
                    optionsNotUsed.Disable();
                }
            }

            RaycastInput input = new RaycastInput(mouseRay, m_mouseRayLength);
            if (mouseRayValid && ToolBase.RayCast(input, out var output))
            {
                switch (m_mode)
                {
                    case Mode.Select:
                        if (!m_zoning && !m_dezoning)
                        {
                            Singleton<NetManager>.instance.GetClosestSegments(output.m_hitPos, m_closeSegments, out m_closeSegmentCount);
                            float distanceSq2 = 256f;
                            ushort block3 = 0;
                            for (int k = 0; k < m_closeSegmentCount; k++)
                            {
                                Singleton<NetManager>.instance.m_segments.m_buffer[m_closeSegments[k]].GetClosestZoneBlock(output.m_hitPos, ref distanceSq2, ref block3);
                            }

                            if (block3 != 0)
                            {
                                ZoneBlock block4 = Singleton<ZoneManager>.instance.m_blocks.m_buffer[block3];
                                Vector3 direction2 = Vector3.forward;
                                ItemClass.Zone zone2 = ItemClass.Zone.Unzoned;
                                bool occupied3 = false;
                                bool occupied4 = false;
                                Snap(ref output.m_hitPos, ref direction2, ref zone2, ref occupied3, ref occupied4, ref block4);
                                while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                                {
                                }

                                try
                                {
                                    m_mouseDirection = direction2;
                                    m_mousePosition = output.m_hitPos;
                                    m_validPosition = true;
                                    break;
                                }
                                finally
                                {
                                    Monitor.Exit(m_dataLock);
                                }
                            }

                            while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                            {
                            }

                            try
                            {
                                m_mouseDirection = cameraDirection;
                                m_mousePosition = output.m_hitPos;
                                m_validPosition = true;
                                break;
                            }
                            finally
                            {
                                Monitor.Exit(m_dataLock);
                            }
                        }

                        while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                        {
                        }

                        try
                        {
                            m_mousePosition = output.m_hitPos;
                            m_validPosition = true;
                            break;
                        }
                        finally
                        {
                            Monitor.Exit(m_dataLock);
                        }
                    case Mode.Brush:
                        while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                        {
                        }

                        try
                        {
                            m_mousePosition = output.m_hitPos;
                            m_validPosition = true;
                        }
                        finally
                        {
                            Monitor.Exit(m_dataLock);
                        }

                        if (m_zoning != m_dezoning)
                        {
                            ApplyBrush();
                        }

                        break;
                    case Mode.Fill:
                        {
                            Singleton<NetManager>.instance.GetClosestSegments(output.m_hitPos, m_closeSegments, out m_closeSegmentCount);
                            float distanceSq = 256f;
                            ushort block = 0;
                            for (int i = 0; i < m_closeSegmentCount; i++)
                            {
                                Singleton<NetManager>.instance.m_segments.m_buffer[m_closeSegments[i]].GetClosestZoneBlock(output.m_hitPos, ref distanceSq, ref block);
                            }

                            if (block != 0)
                            {
                                ZoneBlock block2 = Singleton<ZoneManager>.instance.m_blocks.m_buffer[block];
                                Vector3 direction = Vector3.forward;
                                ItemClass.Zone zone = ItemClass.Zone.Unzoned;
                                bool occupied = false;
                                bool occupied2 = false;
                                Snap(ref output.m_hitPos, ref direction, ref zone, ref occupied, ref occupied2, ref block2);
                                if (CalculateFillBuffer(output.m_hitPos, direction, zone, occupied, occupied2))
                                {
                                    while (!Monitor.TryEnter(m_dataLock, SimulationManager.SYNCHRONIZE_TIMEOUT))
                                    {
                                    }

                                    try
                                    {
                                        for (int j = 0; j < 64; j++)
                                        {
                                            m_fillBuffer2[j] = m_fillBuffer1[j];
                                        }

                                        m_mouseDirection = direction;
                                        m_mousePosition = output.m_hitPos;
                                        m_validPosition = true;
                                        break;
                                    }
                                    finally
                                    {
                                        Monitor.Exit(m_dataLock);
                                    }
                                }

                                m_validPosition = false;
                            }
                            else
                            {
                                m_validPosition = false;
                            }

                            break;
                        }
                }
            }
            else
            {
                m_validPosition = false;
            }
        }

        private bool CalculateFillBuffer(Vector3 position, Vector3 direction, ItemClass.Zone requiredZone, bool occupied1, bool occupied2)
        {
            for (int i = 0; i < 64; i++)
            {
                m_fillBuffer1[i] = 0uL;
            }

            if (!occupied2)
            {
                float angle = Mathf.Atan2(0f - direction.x, direction.z);
                float num = position.x - 256f;
                float num2 = position.z - 256f;
                float num3 = position.x + 256f;
                float num4 = position.z + 256f;
                int num5 = Mathf.Max((int)((num - 46f) / 64f + 75f), 0);
                int num6 = Mathf.Max((int)((num2 - 46f) / 64f + 75f), 0);
                int num7 = Mathf.Min((int)((num3 + 46f) / 64f + 75f), 149);
                int num8 = Mathf.Min((int)((num4 + 46f) / 64f + 75f), 149);
                ZoneManager instance = Singleton<ZoneManager>.instance;
                for (int j = num6; j <= num8; j++)
                {
                    for (int k = num5; k <= num7; k++)
                    {
                        ushort num9 = instance.m_zoneGrid[j * 150 + k];
                        int num10 = 0;
                        while (num9 != 0)
                        {
                            Vector3 position2 = instance.m_blocks.m_buffer[num9].m_position;
                            float num11 = Mathf.Max(Mathf.Max(num - 46f - position2.x, num2 - 46f - position2.z), Mathf.Max(position2.x - num3 - 46f, position2.z - num4 - 46f));
                            if (num11 < 0f)
                            {
                                CalculateFillBuffer(position, direction, angle, num9, ref instance.m_blocks.m_buffer[num9], requiredZone, occupied1, occupied2);
                            }

                            num9 = instance.m_blocks.m_buffer[num9].m_nextGridBlock;
                            if (++num10 >= 49152)
                            {
                                CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                                break;
                            }
                        }
                    }
                }
            }

            if ((m_fillBuffer1[32] & 0x100000000L) != 0)
            {
                m_fillPositions.Clear();
                int num12 = 0;
                int num13 = 32;
                int num14 = 32;
                int num15 = 32;
                int num16 = 32;
                FillPos item = default(FillPos);
                item.m_x = 32;
                item.m_z = 32;
                m_fillPositions.Add(item);
                m_fillBuffer1[32] &= 18446744069414584319uL;
                while (num12 < m_fillPositions.m_size)
                {
                    item = m_fillPositions.m_buffer[num12++];
                    if (item.m_z > 0)
                    {
                        FillPos item2 = item;
                        item2.m_z--;
                        if ((m_fillBuffer1[item2.m_z] & (ulong)(1L << (int)item2.m_x)) != 0)
                        {
                            m_fillPositions.Add(item2);
                            m_fillBuffer1[item2.m_z] &= (ulong)(~(1L << (int)item2.m_x));
                            if (item2.m_z < num14)
                            {
                                num14 = item2.m_z;
                            }
                        }
                    }

                    if (item.m_x > 0)
                    {
                        FillPos item3 = item;
                        item3.m_x--;
                        if ((m_fillBuffer1[item3.m_z] & (ulong)(1L << (int)item3.m_x)) != 0)
                        {
                            m_fillPositions.Add(item3);
                            m_fillBuffer1[item3.m_z] &= (ulong)(~(1L << (int)item3.m_x));
                            if (item3.m_x < num13)
                            {
                                num13 = item3.m_x;
                            }
                        }
                    }

                    if (item.m_x < 63)
                    {
                        FillPos item4 = item;
                        item4.m_x++;
                        if ((m_fillBuffer1[item4.m_z] & (ulong)(1L << (int)item4.m_x)) != 0)
                        {
                            m_fillPositions.Add(item4);
                            m_fillBuffer1[item4.m_z] &= (ulong)(~(1L << (int)item4.m_x));
                            if (item4.m_x > num15)
                            {
                                num15 = item4.m_x;
                            }
                        }
                    }

                    if (item.m_z >= 63)
                    {
                        continue;
                    }

                    FillPos item5 = item;
                    item5.m_z++;
                    if ((m_fillBuffer1[item5.m_z] & (ulong)(1L << (int)item5.m_x)) != 0)
                    {
                        m_fillPositions.Add(item5);
                        m_fillBuffer1[item5.m_z] &= (ulong)(~(1L << (int)item5.m_x));
                        if (item5.m_z > num16)
                        {
                            num16 = item5.m_z;
                        }
                    }
                }

                for (int l = 0; l < 64; l++)
                {
                    m_fillBuffer1[l] = 0uL;
                }

                for (int m = 0; m < m_fillPositions.m_size; m++)
                {
                    FillPos fillPos = m_fillPositions.m_buffer[m];
                    m_fillBuffer1[fillPos.m_z] |= (ulong)(1L << (int)fillPos.m_x);
                }

                return true;
            }

            for (int n = 0; n < 64; n++)
            {
                m_fillBuffer1[n] = 0uL;
            }

            return false;
        }

        private void CalculateFillBuffer(Vector3 position, Vector3 direction, float angle, ushort blockIndex, ref ZoneBlock block, ItemClass.Zone requiredZone, bool occupied1, bool occupied2)
        {
            float num = Mathf.Abs(block.m_angle - angle) * (2f / (float)Math.PI);
            num -= Mathf.Floor(num);
            if (num >= 0.01f && num <= 0.99f)
            {
                return;
            }

            int rowCount = block.RowCount;
            Vector3 vector = new Vector3(Mathf.Cos(block.m_angle), 0f, Mathf.Sin(block.m_angle)) * 8f;
            Vector3 vector2 = new Vector3(vector.z, 0f, 0f - vector.x);
            for (int i = 0; i < rowCount; i++)
            {
                Vector3 vector3 = ((float)i - 3.5f) * vector2;
                for (int j = 0; j < 4; j++)
                {
                    if ((block.m_valid & (ulong)(1L << ((i << 3) | j))) == 0 || block.GetZone(j, i) != requiredZone)
                    {
                        continue;
                    }

                    if (occupied1)
                    {
                        if (requiredZone == ItemClass.Zone.Unzoned && (block.m_occupied1 & (ulong)(1L << ((i << 3) | j))) == 0)
                        {
                            continue;
                        }
                    }
                    else if (occupied2)
                    {
                        if (requiredZone == ItemClass.Zone.Unzoned && (block.m_occupied2 & (ulong)(1L << ((i << 3) | j))) == 0)
                        {
                            continue;
                        }
                    }
                    else if (((block.m_occupied1 | block.m_occupied2) & (ulong)(1L << ((i << 3) | j))) != 0)
                    {
                        continue;
                    }

                    Vector3 vector4 = ((float)j - 3.5f) * vector;
                    Vector3 vector5 = block.m_position + vector4 + vector3 - position;
                    float num2 = (vector5.x * direction.x + vector5.z * direction.z) * 0.125f + 32f;
                    float num3 = (vector5.x * direction.z - vector5.z * direction.x) * 0.125f + 32f;
                    int num4 = Mathf.RoundToInt(num2);
                    int num5 = Mathf.RoundToInt(num3);
                    if (num4 >= 0 && num4 < 64 && num5 >= 0 && num5 < 64 && !(Mathf.Abs(num2 - (float)num4) >= 0.0125f) && !(Mathf.Abs(num3 - (float)num5) >= 0.0125f))
                    {
                        m_fillBuffer1[num5] |= (ulong)(1L << num4);
                    }
                }
            }
        }

        private void ApplyFill()
        {
            if (!m_validPosition)
            {
                return;
            }

            Vector3 mousePosition = m_mousePosition;
            Vector3 mouseDirection = m_mouseDirection;
            float angle = Mathf.Atan2(0f - mouseDirection.x, mouseDirection.z);
            float num = mousePosition.x - 256f;
            float num2 = mousePosition.z - 256f;
            float num3 = mousePosition.x + 256f;
            float num4 = mousePosition.z + 256f;
            int num5 = Mathf.Max((int)((num - 46f) / 64f + 75f), 0);
            int num6 = Mathf.Max((int)((num2 - 46f) / 64f + 75f), 0);
            int num7 = Mathf.Min((int)((num3 + 46f) / 64f + 75f), 149);
            int num8 = Mathf.Min((int)((num4 + 46f) / 64f + 75f), 149);
            ZoneManager instance = Singleton<ZoneManager>.instance;
            bool flag = false;
            for (int i = num6; i <= num8; i++)
            {
                for (int j = num5; j <= num7; j++)
                {
                    ushort num9 = instance.m_zoneGrid[i * 150 + j];
                    int num10 = 0;
                    while (num9 != 0)
                    {
                        Vector3 position = instance.m_blocks.m_buffer[num9].m_position;
                        float num11 = Mathf.Max(Mathf.Max(num - 46f - position.x, num2 - 46f - position.z), Mathf.Max(position.x - num3 - 46f, position.z - num4 - 46f));
                        if (num11 < 0f && ApplyFillBuffer(mousePosition, mouseDirection, angle, num9, ref instance.m_blocks.m_buffer[num9]))
                        {
                            flag = true;
                        }

                        num9 = instance.m_blocks.m_buffer[num9].m_nextGridBlock;
                        if (++num10 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            if (flag)
            {
                if (m_zoning)
                {
                    UsedZone(m_zone);
                }

                EffectInfo fillEffect = instance.m_properties.m_fillEffect;
                if ((object)fillEffect != null)
                {
                    EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea(mousePosition, Vector3.up, 1f);
                    Singleton<EffectManager>.instance.DispatchEffect(fillEffect, spawnArea, Vector3.zero, 0f, 1f, Singleton<AudioManager>.instance.DefaultGroup, 0u, avoidMultipleAudio: true);
                }
            }
        }

        private bool ApplyFillBuffer(Vector3 position, Vector3 direction, float angle, ushort blockIndex, ref ZoneBlock block)
        {
            int rowCount = block.RowCount;
            Vector3 vector = new Vector3(Mathf.Cos(block.m_angle), 0f, Mathf.Sin(block.m_angle)) * 8f;
            Vector3 vector2 = new Vector3(vector.z, 0f, 0f - vector.x);
            bool flag = false;
            for (int i = 0; i < rowCount; i++)
            {
                Vector3 vector3 = ((float)i - 3.5f) * vector2;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 vector4 = ((float)j - 3.5f) * vector;
                    Vector3 vector5 = block.m_position + vector4 + vector3 - position;
                    float num = (vector5.x * direction.x + vector5.z * direction.z) * 0.125f + 32f;
                    float num2 = (vector5.x * direction.z - vector5.z * direction.x) * 0.125f + 32f;
                    int num3 = Mathf.Clamp(Mathf.RoundToInt(num), 0, 63);
                    int num4 = Mathf.Clamp(Mathf.RoundToInt(num2), 0, 63);
                    bool flag2 = false;
                    for (int k = -1; k <= 1; k++)
                    {
                        if (flag2)
                        {
                            break;
                        }

                        for (int l = -1; l <= 1; l++)
                        {
                            if (flag2)
                            {
                                break;
                            }

                            int num5 = num3 + l;
                            int num6 = num4 + k;
                            if (num5 < 0 || num5 >= 64 || num6 < 0 || num6 >= 64 || (num - (float)num5) * (num - (float)num5) + (num2 - (float)num6) * (num2 - (float)num6) >= 0.5625f || (m_fillBuffer1[num6] & (ulong)(1L << num5)) == 0)
                            {
                                continue;
                            }

                            if (m_zoning)
                            {
                                if ((m_zone == ItemClass.Zone.Unzoned || block.GetZone(j, i) == ItemClass.Zone.Unzoned) && block.SetZone(j, i, m_zone))
                                {
                                    flag = true;
                                }
                            }
                            else if (m_dezoning && block.SetZone(j, i, ItemClass.Zone.Unzoned))
                            {
                                flag = true;
                            }

                            flag2 = true;
                        }
                    }
                }
            }

            if (flag)
            {
                block.RefreshZoning(blockIndex);
                return true;
            }

            return false;
        }

        public void ApplyZoning()
        {
            Vector2 vector = VectorUtils.XZ(m_startPosition);
            Vector2 vector2 = VectorUtils.XZ(m_mousePosition);
            Vector2 vector3 = VectorUtils.XZ(m_startDirection);
            Vector2 vector4 = new Vector2(vector3.y, 0f - vector3.x);
            float num = Mathf.Round(((vector2.x - vector.x) * vector3.x + (vector2.y - vector.y) * vector3.y) * 0.125f) * 8f;
            float num2 = Mathf.Round(((vector2.x - vector.x) * vector4.x + (vector2.y - vector.y) * vector4.y) * 0.125f) * 8f;
            float num3 = ((!(num >= 0f)) ? (-4f) : 4f);
            float num4 = ((!(num2 >= 0f)) ? (-4f) : 4f);
            Quad2 quad = default(Quad2);
            quad.a = vector - vector3 * num3 - vector4 * num4;
            quad.b = vector - vector3 * num3 + vector4 * (num2 + num4);
            quad.c = vector + vector3 * (num + num3) + vector4 * (num2 + num4);
            quad.d = vector + vector3 * (num + num3) - vector4 * num4;
            if (num3 == num4)
            {
                Vector2 b = quad.b;
                quad.b = quad.d;
                quad.d = b;
            }

            Vector2 vector5 = quad.Min();
            Vector2 vector6 = quad.Max();
            ZoneManager instance = Singleton<ZoneManager>.instance;
            int num5 = Mathf.Max((int)((vector5.x - 46f) / 64f + 75f), 0);
            int num6 = Mathf.Max((int)((vector5.y - 46f) / 64f + 75f), 0);
            int num7 = Mathf.Min((int)((vector6.x + 46f) / 64f + 75f), 149);
            int num8 = Mathf.Min((int)((vector6.y + 46f) / 64f + 75f), 149);
            bool flag = false;

            for (int i = num6; i <= num8; i++)
            {
                for (int j = num5; j <= num7; j++)
                {
                    ushort num9 = instance.m_zoneGrid[i * 150 + j];
                    int num10 = 0;
                    while (num9 != 0)
                    {
                        Vector3 position = instance.m_blocks.m_buffer[num9].m_position;
                        float num11 = Mathf.Max(Mathf.Max(vector5.x - 46f - position.x, vector5.y - 46f - position.z), Mathf.Max(position.x - vector6.x - 46f, position.z - vector6.y - 46f));
                        if (num11 < 0f && ApplyZoning(num9, ref instance.m_blocks.m_buffer[num9], quad))
                        {
                            flag = true;
                        }

                        num9 = instance.m_blocks.m_buffer[num9].m_nextGridBlock;
                        if (++num10 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            if (flag)
            {
                if (m_zoning)
                {
                    UsedZone(m_zone);
                }

                EffectInfo fillEffect = instance.m_properties.m_fillEffect;
                if ((object)fillEffect != null)
                {
                    EffectInfo.SpawnArea spawnArea = new EffectInfo.SpawnArea((vector + vector2) * 0.5f, Vector3.up, 1f);
                    Singleton<EffectManager>.instance.DispatchEffect(fillEffect, spawnArea, Vector3.zero, 0f, 1f, Singleton<AudioManager>.instance.DefaultGroup, 0u, avoidMultipleAudio: true);
                }
            }
        }

        private bool ApplyZoning(ushort blockIndex, ref ZoneBlock data, Quad2 quad2)
        {
            int rowCount = data.RowCount;
            Vector2 vector = new Vector2(Mathf.Cos(data.m_angle), Mathf.Sin(data.m_angle)) * 8f;
            Vector2 vector2 = new Vector2(vector.y, 0f - vector.x);
            Vector2 vector3 = VectorUtils.XZ(data.m_position);
            Quad2 quad3 = default(Quad2);
            quad3.a = vector3 - 4f * vector - 4f * vector2;
            quad3.b = vector3 + 4f * vector - 4f * vector2;
            quad3.c = vector3 + 4f * vector + (rowCount - 4) * vector2;
            quad3.d = vector3 - 4f * vector + (rowCount - 4) * vector2;
            if (!quad3.Intersect(quad2))
            {
                return false;
            }

            bool flag = false;
            for (int i = 0; i < rowCount; i++)
            {
                Vector2 vector4 = ((float)i - 3.5f) * vector2;
                for (int j = 0; j < 4; j++)
                {
                    Vector2 vector5 = ((float)j - 3.5f) * vector;
                    Vector2 p = vector3 + vector5 + vector4;
                    if (!quad2.Intersect(p))
                    {
                        continue;
                    }

                    if (m_zoning)
                    {
                        Debug.Log($"OZoneTool->ApplyZoning>>Before: {data.GetZone(j, i)}");
                        if ((m_zone == ItemClass.Zone.Unzoned || data.GetZone(j, i) == ItemClass.Zone.Unzoned) && data.SetZone(j, i, m_zone))
                        {
                            Debug.Log($"OZoneTool->ApplyZoning>>After: {data.GetZone(j, i)}");
                            flag = true;
                        }
                    }
                    else if (m_dezoning && data.SetZone(j, i, ItemClass.Zone.Unzoned))
                    {
                        flag = true;
                    }
                }
            }

            if (flag)
            {
                data.RefreshZoning(blockIndex);
                return true;
            }

            return false;
        }

        private void UsedZone(ItemClass.Zone zone)
        {
            if (zone != ItemClass.Zone.None)
            {
                ZoneManager instance = Singleton<ZoneManager>.instance;
                instance.m_zonesNotUsed.Disable();
                instance.m_zoneNotUsed[(int)zone].Disable();
                switch (zone)
                {
                    case ItemClass.Zone.ResidentialLow:
                    case ItemClass.Zone.ResidentialHigh:
                        instance.m_zoneDemandResidential.Deactivate();
                        break;
                    case ItemClass.Zone.CommercialLow:
                    case ItemClass.Zone.CommercialHigh:
                        instance.m_zoneDemandCommercial.Deactivate();
                        break;
                    case ItemClass.Zone.Industrial:
                    case ItemClass.Zone.Office:
                        instance.m_zoneDemandWorkplace.Deactivate();
                        break;
                }
            }
        }

        private void ApplyBrush(ushort blockIndex, ref ZoneBlock data, Vector3 position, float brushRadius)
        {
            Vector3 vector = data.m_position - position;
            if (Mathf.Abs(vector.x) > 46f + brushRadius || Mathf.Abs(vector.z) > 46f + brushRadius)
            {
                return;
            }

            int num = (int)((data.m_flags & 0xFF00) >> 8);
            Vector3 vector2 = new Vector3(Mathf.Cos(data.m_angle), 0f, Mathf.Sin(data.m_angle)) * 8f;
            Vector3 vector3 = new Vector3(vector2.z, 0f, 0f - vector2.x);
            bool flag = false;
            for (int i = 0; i < num; i++)
            {
                Vector3 vector4 = ((float)i - 3.5f) * vector3;
                for (int j = 0; j < 4; j++)
                {
                    Vector3 vector5 = ((float)j - 3.5f) * vector2;
                    Vector3 vector6 = vector + vector5 + vector4;
                    float num2 = vector6.x * vector6.x + vector6.z * vector6.z;
                    if (!(num2 <= brushRadius * brushRadius))
                    {
                        continue;
                    }

                    if (m_zoning)
                    {
                        if ((m_zone == ItemClass.Zone.Unzoned || data.GetZone(j, i) == ItemClass.Zone.Unzoned) && data.SetZone(j, i, m_zone))
                        {
                            flag = true;
                        }
                    }
                    else if (m_dezoning && data.SetZone(j, i, ItemClass.Zone.Unzoned))
                    {
                        flag = true;
                    }
                }
            }

            if (flag)
            {
                data.RefreshZoning(blockIndex);
                if (m_zoning)
                {
                    UsedZone(m_zone);
                }
            }
        }

        private void ApplyBrush()
        {
            float num = m_brushSize * 0.5f;
            Vector3 mousePosition = m_mousePosition;
            float num2 = mousePosition.x - num;
            float num3 = mousePosition.z - num;
            float num4 = mousePosition.x + num;
            float num5 = mousePosition.z + num;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            int num6 = Mathf.Max((int)((num2 - 46f) / 64f + 75f), 0);
            int num7 = Mathf.Max((int)((num3 - 46f) / 64f + 75f), 0);
            int num8 = Mathf.Min((int)((num4 + 46f) / 64f + 75f), 149);
            int num9 = Mathf.Min((int)((num5 + 46f) / 64f + 75f), 149);
            for (int i = num7; i <= num9; i++)
            {
                for (int j = num6; j <= num8; j++)
                {
                    ushort num10 = instance.m_zoneGrid[i * 150 + j];
                    int num11 = 0;
                    while (num10 != 0)
                    {
                        Vector3 position = instance.m_blocks.m_buffer[num10].m_position;
                        float num12 = Mathf.Max(Mathf.Max(num2 - 46f - position.x, num3 - 46f - position.z), Mathf.Max(position.x - num4 - 46f, position.z - num5 - 46f));
                        if (num12 < 0f)
                        {
                            ApplyBrush(num10, ref instance.m_blocks.m_buffer[num10], mousePosition, num);
                        }

                        num10 = instance.m_blocks.m_buffer[num10].m_nextGridBlock;
                        if (++num11 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
        }
    }
}
