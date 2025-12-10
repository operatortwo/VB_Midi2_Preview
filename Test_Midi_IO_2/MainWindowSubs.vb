Partial Public Class MainWindow
    Private Sub WriteMessageLine(str As String)
        If str Is Nothing Then Exit Sub
        TbMessage.AppendText(str)
        TbMessage.AppendText(vbCrLf)
    End Sub

End Class
