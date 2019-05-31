using System;

public interface IBuildPreviewer
{
    Action OnSnapToGrid { get; set; }
    Action OnConfirm { get; set; }
    Action OnCancel { get; set; }
}
