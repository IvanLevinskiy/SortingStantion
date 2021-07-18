using S7Communication.Enumerations;
using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace S7Communication.Utilites
{
    public class Converters
    {
        /// <summary>
        /// Преобразование XmlNode в SimaticType
        /// </summary>
        /// <param name="node"></param>
        /// <param name="XmlNodeStr"></param>
        /// <returns></returns>
        public static SimaticType XmlNodeToSimaticType(XmlNode node, string XmlNodeStr)
        {
            try
            {
                XmlNode attribut = node.Attributes.GetNamedItem(XmlNodeStr);

                switch (attribut.Value)
                {
                    case "BOOL": return SimaticType.BOOL; break;
                    case "BYTE": return SimaticType.BYTE; break;
                    case "WORD": return SimaticType.WORD; break;
                    case "DWORD": return SimaticType.DWORD; break;
                    case "REAL": return SimaticType.REAL; break;
                    case "TIMER": return SimaticType.TIMER; break;
                    case "COUNTER": return SimaticType.COUNTER; break;
                    case "DTL": return SimaticType.DTL; break;
                    case "AUTO": return SimaticType.AUTO; break;
                }
            }
            catch (Exception ex)
            {

            }
            
            return SimaticType.AUTO;
        }


        /// <summary>
        /// Преобразование XmlNode в string 
        /// </summary>
        /// <returns></returns>
        public static string XmlNodeToString(XmlNode node, string XmlNodeStr)
        {
            try
            {
                XmlNode attribut = node.Attributes.GetNamedItem(XmlNodeStr);

                if (attribut == null)
                {
                    return string.Empty;
                }

                if (attribut.Value != string.Empty)
                {
                    return attribut.Value;
                }
            }
            catch (Exception)
            {

            }

            return string.Empty;
        }

        /// <summary>
        /// Преобразование XmlNode в int
        /// </summary>
        /// <returns></returns>
        public static int XmlNodeToInt(XmlNode node, string XmlNodeStr)
        {

            try
            {
                XmlNode attribut = node.Attributes.GetNamedItem(XmlNodeStr);

                if (attribut.Value != string.Empty)
                {
                    return Convert.ToInt16(attribut.Value);
                }
            }
            catch (Exception ex)
            {

            }

            return -1;
        }

        /// <summary>
        /// Преобразование XmlNode в CpuType
        /// </summary>
        /// <param name="node"></param>
        /// <param name="XmlNodeStr"></param>
        /// <returns></returns>
        public static CpuType XmlNodeToCpuType(XmlNode node, string XmlNodeStr)
        {
            try
            {
                XmlNode attribut = node.Attributes.GetNamedItem(XmlNodeStr);

                //Возвращение значения по умолчанию
                if (attribut == null)
                {
                    return CpuType.S7300;
                }


                if (attribut.Value != string.Empty)
                {
                    switch (attribut.Value)
                    {
                        case "S7200": return CpuType.S7200; break;
                        case "S7300": return CpuType.S7300; break;
                        case "S7400": return CpuType.S7400; break;
                        case "S71200": return CpuType.S71200; break;
                        case "S71500": return CpuType.S71500; break;
                    }
                }
            }
            catch (Exception)
            {

            }

            return CpuType.S7300;
        }

        /// <summary>
        /// Метод преобразующий строку в DataType
        /// </summary>
        /// <param name="Address"></param>
        /// <returns></returns>
        public static MemmoryArea StringToDataType(string Address)
        {
            if (Address.Contains("DB"))
            {
                return MemmoryArea.DataBlock;
            }

            if (Address.Contains("M"))
            {
                return MemmoryArea.Memory;
            }

            if (Address.Contains("I"))
            {
                return MemmoryArea.Input;
            }

            if (Address.Contains("T"))
            {
                return MemmoryArea.Timer;
            }

            if (Address.Contains("Q"))
            {
                return MemmoryArea.Output;
            }

            return MemmoryArea.Memory;

        }

        /// <summary>
        /// Метод для предобазования ObservableCollection<SimaticTag>
        /// в SimaticTag[]
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static simaticTagBase[]  CollectionToArray(ObservableCollection<simaticTagBase> tags)
        {
            simaticTagBase[] array = new simaticTagBase[tags.Count];

            for (int i = 0; i < tags.Count; i ++)
            {
                array[i] = tags[i];
            }

            return array;

        }

    }
}
