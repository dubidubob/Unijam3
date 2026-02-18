using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class WindowManager : MonoBehaviour
{
    // Windows API 호출을 위한 선언
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left, Top, Right, Bottom; }


    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    // 현재 창의 좌표와 크기를 한 번에 가져오는 함수
    public static RECT GetCurrentWindowPos()
    {
        IntPtr hWnd = GetActiveWindow();
        GetWindowRect(hWnd, out RECT rect);
        return rect;
    }

    public static void ResizeWindow(int width, int height)
    {
        IntPtr hWnd = GetActiveWindow();
        if (hWnd != IntPtr.Zero)
        {
            // SWP_NOMOVE를 써서 위치는 고정하고 크기만 바꿈
            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, width, height, SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE);
        }
    }

    // 중앙 기준으로 크기 조절
    public static void ResizeCentered(int targetW, int targetH, RECT startRect)
    {
        IntPtr hWnd = GetActiveWindow();
        if (hWnd == IntPtr.Zero) return;

        int baseW = startRect.Right - startRect.Left;
        int baseH = startRect.Bottom - startRect.Top;

        // 중앙점을 유지하기 위한 새로운 X, Y 좌표 계산
        int newX = startRect.Left - (targetW - baseW) / 2;
        int newY = startRect.Top - (targetH - baseH) / 2;

        SetWindowPos(hWnd, IntPtr.Zero, newX, newY, targetW, targetH, SWP_NOZORDER | SWP_NOACTIVATE);
    }

}