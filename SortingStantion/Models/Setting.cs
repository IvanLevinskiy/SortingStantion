﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SortingStantion.Models
{
    /// <summary>
    /// Класс, реализующий настройку из 
    /// файла конфигурации
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Наименование настройки
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Значение настройки
        /// </summary>
        public string Value
        {
            get;
            set;
        }

        /// <summary>
        /// Указатель на узел в файле
        /// настроек
        /// </summary>
        XmlNode Node;

        /// <summary>
        /// Конструктор
        /// </summary>
        public Setting(XmlNode node)
        {
            //Получение указателя на XmlNode
            Node = node;

            //Получение наименования узла
            Name = GetAttribut("name");

            //Получение значения настройки
            Value = Node.InnerText;
        }

        /// <summary>
        /// Получение атрибута из XmlNode
        /// </summary>
        /// <param name="node"></param>
        /// <param name="attribut"></param>
        /// <returns></returns>
        public string GetAttribut(string attribut)
        {
            try
            {
                if (Node.Attributes == null)
                {
                    return string.Empty;
                }

                XmlNode attributNode = Node.Attributes.GetNamedItem(attribut);


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
    }
}
