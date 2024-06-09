using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface IDynamicScrollContainer<T>
{
    public void UpdateDatas(List<T> datas);
}
