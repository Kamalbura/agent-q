using System;
using System.Collections.Generic;

namespace Assistant.Core.Context;

public readonly record struct Rect(double X, double Y, double Width, double Height)
{
    public bool Contains(Rect other) =>
        other.X >= X &&
        other.Y >= Y &&
        other.X + other.Width <= X + Width &&
        other.Y + other.Height <= Y + Height;

    public bool Contains(double x, double y) =>
        x >= X && y >= Y && x <= X + Width && y <= Y + Height;
}

public readonly record struct Bounds(double X, double Y, double Width, double Height)
{
    public Rect ToRect() => new(X, Y, Width, Height);
}

public sealed record UiElementDto(
    string Id,
    string? AutomationId,
    string Name,
    string ControlType,
    Rect BoundingRectangle,
    IReadOnlyList<UiElementDto> Children,
    bool IsFocusable);

public sealed record ScreenContextDto(
    string ScreenshotPath,
    string ScreenshotHash,
    byte[]? ScreenshotBytes,
    string OcrText,
    IReadOnlyList<UiElementDto> UiTree,
    string FocusedWindowTitle,
    Rect ScreenBounds,
    DateTime TimestampUtc,
    string? PreviousActionsSummary);
