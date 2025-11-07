<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        TbMessage = New TextBox()
        Label1 = New Label()
        TextBox1 = New TextBox()
        BtnClose = New Button()
        BtnSendMsg = New Button()
        SuspendLayout()
        ' 
        ' TbMessage
        ' 
        TbMessage.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        TbMessage.BorderStyle = BorderStyle.FixedSingle
        TbMessage.Location = New Point(63, 122)
        TbMessage.Multiline = True
        TbMessage.Name = "TbMessage"
        TbMessage.ReadOnly = True
        TbMessage.Size = New Size(673, 286)
        TbMessage.TabIndex = 0
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(63, 104)
        Label1.Name = "Label1"
        Label1.Size = New Size(53, 15)
        Label1.TabIndex = 1
        Label1.Text = "Message"
        ' 
        ' TextBox1
        ' 
        TextBox1.BackColor = Color.Ivory
        TextBox1.BorderStyle = BorderStyle.FixedSingle
        TextBox1.Location = New Point(147, 26)
        TextBox1.Multiline = True
        TextBox1.Name = "TextBox1"
        TextBox1.ReadOnly = True
        TextBox1.Size = New Size(455, 69)
        TextBox1.TabIndex = 2
        TextBox1.Text = resources.GetString("TextBox1.Text")
        ' 
        ' BtnClose
        ' 
        BtnClose.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom
        BtnClose.Location = New Point(352, 450)
        BtnClose.Name = "BtnClose"
        BtnClose.Size = New Size(75, 23)
        BtnClose.TabIndex = 3
        BtnClose.Text = "Close"
        BtnClose.UseVisualStyleBackColor = True
        ' 
        ' BtnSendMsg
        ' 
        BtnSendMsg.Enabled = False
        BtnSendMsg.Location = New Point(644, 56)
        BtnSendMsg.Name = "BtnSendMsg"
        BtnSendMsg.Size = New Size(108, 27)
        BtnSendMsg.TabIndex = 4
        BtnSendMsg.Text = "Send Message"
        BtnSendMsg.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        AutoScroll = True
        ClientSize = New Size(800, 495)
        Controls.Add(BtnSendMsg)
        Controls.Add(BtnClose)
        Controls.Add(TextBox1)
        Controls.Add(Label1)
        Controls.Add(TbMessage)
        Name = "Form1"
        Text = "Basics Winforms"
        ResumeLayout(False)
        PerformLayout()
    End Sub

    Friend WithEvents TbMessage As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents TextBox1 As TextBox
    Friend WithEvents BtnClose As Button
    Friend WithEvents BtnSendMsg As Button

End Class
