using System;

namespace Perspex.Controls
{
    public interface IVirtualizingPanel : IPanel
    {
        bool IsFull { get; }

        int OverflowCount { get; }

        Action ArrangeCompleted { get; set; }
    }
}
