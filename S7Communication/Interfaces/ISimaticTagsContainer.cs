using S7Communication;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Simatic.Interfaces
{
    public interface ISimaticTagsContainer
    {
        /// <summary>
        /// Метод для получения тэгов
        /// </summary>
        /// <returns></returns>
        ObservableCollection<simaticTagBase> GetTags();
    }
}
