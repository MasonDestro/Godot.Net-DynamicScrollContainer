using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IDynamicContainerItem<T>
{
    public T Data { get; }
    public void UpdateData(T data);
}
