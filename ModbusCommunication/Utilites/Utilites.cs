using System;
using System.Xml;

namespace Communication
{
    public static class Utilites
    {
        /// <summary>
        /// Получение атрибута из XmlNode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribut"></param>
        /// <returns></returns>
        public static string GetAttributString(XmlNode node, string attribut)
        {
            try
            {
                if (node.Attributes == null)
                {
                    return string.Empty;
                }

                XmlNode attributNode = node.Attributes.GetNamedItem(attribut);


                //Если атрибута нет
                if (attributNode == null)
                {
                    return string.Empty;
                }

                //Если атрибут не пустой
                if (attributNode.Value != string.Empty)
                {
                    return attributNode.Value;
                }

            }
            catch (Exception ex)
            {

            }

            return string.Empty;
        }

        /// <summary>
        /// Получение атрибута int из XmlNode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribut"></param>
        /// <returns></returns>
        public static int GetAttributInt(XmlNode node, string attribut)
        {
            try
            {
                if (node.Attributes == null)
                {
                    return 0;
                }

                XmlNode attributNode = node.Attributes.GetNamedItem(attribut);

                if (attributNode == null)
                {
                    return 0;
                }

                if (attributNode.Value != string.Empty)
                {
                    return Convert.ToInt16(attributNode.Value);
                }

            }
            catch (Exception ex)
            {

            }

            return 0;
        }

        /// <summary>
        /// Получение атрибута double из XmlNode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribut"></param>
        /// <returns></returns>
        public static double GetAttributDouble(XmlNode node, string attribut)
        {
            try
            {
                if (node.Attributes == null)
                {
                    return 0;
                }

                XmlNode attributNode = node.Attributes.GetNamedItem(attribut);

                if (attributNode == null)
                {
                    return 0D;
                }

                if (attributNode.Value != string.Empty)
                {
                    return Convert.ToDouble(attributNode.Value.Replace(".", ","));
                }

            }
            catch (Exception ex)
            {

            }

            return 0D;
        }

        /// <summary>
        /// Метод для получения адреса регистра
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string ClearAddressStr(string address)
        {
            string temp = address;

            if (temp.Contains(":"))
            {
                temp = temp.Split(':')[1];
            }

            if (temp.Contains("-"))
            {
                temp = temp.Split('-')[0];
            }

            if (temp.Contains("."))
            {
                temp = temp.Split('.')[0];
            }

            return temp;
        }


        public static string GetPortStr(string address)
        {
            string temp = address;

            if (temp.Contains(":"))
            {
                temp = temp.Split(':')[0];
                return temp;
            }

            return "0";
        }

    }
}
