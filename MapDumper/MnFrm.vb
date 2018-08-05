﻿Imports System.IO
Imports VB = Microsoft.VisualBasic

Public Class MnFrm

    Public OutPutHeaderText As String


    Public outputtextFooter As String
    Public Outputprimaryts As String
    Public Outputsecondaryts As String
    Public Outputmetatiles As String
    Public Outputgraphicsfile As String
    Public LayoutsTableText As String
    Public LayoutsText As String

    Public outputlevel2 As String
    Public outputlevel4 As String

    Private Sub LoadButton_Click(sender As Object, e As EventArgs) Handles LoadButton.Click
        fileOpenDialog.FileName = ""
        fileOpenDialog.CheckFileExists = True

        ' Check to ensure that the selected path exists.  Dialog box displays 
        ' a warning otherwise.
        fileOpenDialog.CheckPathExists = True

        ' Get or set default extension. Doesn't include the leading ".".
        fileOpenDialog.DefaultExt = "GBA"

        ' Return the file referenced by a link? If False, simply returns the selected link
        ' file. If True, returns the file linked to the LNK file.
        fileOpenDialog.DereferenceLinks = True

        ' Just as in VB6, use a set of pairs of filters, separated with "|". Each 
        ' pair consists of a description|file spec. Use a "|" between pairs. No need to put a
        ' trailing "|". You can set the FilterIndex property as well, to select the default
        ' filter. The first filter is numbered 1 (not 0). The default is 1. 
        fileOpenDialog.Filter =
            "(*.gba)|*.gba*"

        fileOpenDialog.Multiselect = False

        ' Restore the original directory when done selecting
        ' a file? If False, the current directory changes
        ' to the directory in which you selected the file.
        ' Set this to True to put the current folder back
        ' where it was when you started.
        ' The default is False.
        '.RestoreDirectory = False

        ' Show the Help button and Read-Only checkbox?
        fileOpenDialog.ShowHelp = False
        fileOpenDialog.ShowReadOnly = False

        ' Start out with the read-only check box checked?
        ' This only make sense if ShowReadOnly is True.
        fileOpenDialog.ReadOnlyChecked = False

        fileOpenDialog.Title = "Select ROM to open:"

        ' Only accept valid Win32 file names?
        fileOpenDialog.ValidateNames = True


        If fileOpenDialog.ShowDialog = DialogResult.OK Then

            LoadedROM = fileOpenDialog.FileName

            HandleOpenedROM()

        End If
    End Sub

    Private Sub HandleOpenedROM()
        FileNum = FreeFile()

        FileOpen(FileNum, LoadedROM, OpenMode.Binary)
        'Opens the ROM as binary
        FileGet(FileNum, header, &HAD, True)
        header2 = Mid(header, 1, 3)
        header3 = Mid(header, 4, 1)
        FileClose(FileNum)

        If header2 = "BPR" Or header2 = "BPG" Or header2 = "BPE" Or header2 = "AXP" Or header2 = "AXV" Then
            If header3 = "J" Then
                ROMNameLabel.Text = ""
                LoadedROM = ""
                MessageBox.Show("I haven't added Jap support out of pure lazziness. I will though if it get's highly Demanded.")
                End
            Else
                ROMNameLabel.Text = header & " - " & GetString(GetINIFileLocation(), header, "ROMName", "")
            End If
        Else
            ROMNameLabel.Text = ""
            LoadedROM = ""
            MessageBox.Show("Not one of the Pokemon games...")
            End
        End If

        LoadMapList()
        LoadBanksAndMaps()

    End Sub

    Private Sub LoadMapList()
        MapNameList.Items.Clear()
        Dim i As Integer
        For i = 0 To (GetString((AppPath & "ini\roms.ini"), header, "NumberOfMapLabels", "")) - 1
            MapNameList.Items.Add(GetMapLabelName(i))
        Next i
    End Sub

    Private Sub LoadBanksAndMaps()

        MapsAndBanks.Nodes.Clear()

        Point2MapBankPointers = Int32.Parse(GetString(GetINIFileLocation(), header, "Pointer2PointersToMapBanks", ""), System.Globalization.NumberStyles.HexNumber)

        MapBankPointers = ((Val(("&H" & ReverseHEX(ReadHEX(LoadedROM, Point2MapBankPointers, 4)))) - &H8000000))

        i = 0

        While (ReadHEX(LoadedROM, MapBankPointers + (i * 4), "4") <> "02000000") And (ReadHEX(LoadedROM, MapBankPointers + (i * 4), "4") <> "FFFFFFFF") 'And ((("&H" & ReverseHEX(ReadHEX(LoadedROM, MapBankPointers + (i * 4), 4)))) < &H8000000)

            MapsAndBanks.Nodes.Add(i)

            Dim OriginalBankPointer As String = GetString((AppPath & "ini\roms.ini"), header, ("OriginalBankPointer" & i), "")
            Dim NumberOfMapsInBank As String = GetString((AppPath & "ini\roms.ini"), header, ("NumberOfMapsInBank" & i), "")


            x = 0

            BankPointer = ((Val(("&H" & ReverseHEX(ReadHEX(LoadedROM, MapBankPointers + (i * 4), 4)))) - &H8000000))

            While (x <= 299)

                HeaderPointer = ((Val(("&H" & ReverseHEX(ReadHEX(LoadedROM, BankPointer + (x * 4), 4)))) - &H8000000))

                If (ReadHEX(LoadedROM, BankPointer + (x * 4), 4) = "F7F7F7F7") Then
                    Exit While
                End If

                If OriginalBankPointer = Hex(BankPointer) Then

                    Dim maplabelvar As Integer

                    maplabelvar = CInt((Val(("&H" & ReadHEX(LoadedROM, HeaderPointer + 20, 1)))))

                    If ((header2 = "BPR") Or (header2 = "BPG")) Then

                        MapsAndBanks.Nodes.Item(i).Nodes.Add(New TreeNode(x & " - " & MapNameList.Items.Item(maplabelvar - &H58)))

                    ElseIf (mMain.header2 = "BPE") Then

                        MapsAndBanks.Nodes.Item(i).Nodes.Add(New TreeNode(x & " - " & MapNameList.Items.Item(maplabelvar)))

                    ElseIf ((mMain.header2 = "AXP") Or (mMain.header2 = "AXV")) Then

                        MapsAndBanks.Nodes.Item(i).Nodes.Add(New TreeNode(x & " - " & MapNameList.Items.Item(maplabelvar)))

                    End If
                    'MapsAndBanks.Nodes.Item(i).Nodes.Add(New TreeNode(x & " - " & GetMapLabelName(1)))

                    If NumberOfMapsInBank = x Then

                        Exit While

                    End If

                Else

                    If (ReadHEX(LoadedROM, BankPointer + (x * 4), 4) = "77777777") Then
                        MapsAndBanks.Nodes.Item(i).Nodes.Add(New TreeNode((x & " - Reserved")))
                    Else

                        Dim maplabelvar As Integer

                        maplabelvar = CInt((Val(("&H" & ReadHEX(LoadedROM, HeaderPointer + 20, 1)))))

                        If ((header2 = "BPR") Or (header2 = "BPG")) Then

                            MapsAndBanks.Nodes.Item(i).Nodes.Add(New TreeNode(x & " - " & MapNameList.Items.Item(maplabelvar - &H58)))

                        ElseIf (mMain.header2 = "BPE") Then

                            MapsAndBanks.Nodes.Item(i).Nodes.Add(New TreeNode(x & " - " & MapNameList.Items.Item(maplabelvar)))

                        ElseIf ((mMain.header2 = "AXP") Or (mMain.header2 = "AXV")) Then

                            MapsAndBanks.Nodes.Item(i).Nodes.Add(New TreeNode(x & " - " & MapNameList.Items.Item(maplabelvar)))

                        End If

                    End If

                End If

                x = x + 1
            End While

            i = i + 1

        End While


    End Sub

    Private Sub ExportBttn_Click2(sender As Object, e As EventArgs) Handles ExportBttn.Click

        FolderBrowserDialog1.Description = "Select folder to export to:"

        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            SelectedPath = FolderBrowserDialog1.SelectedPath

            Me.Text = "Please wait..."
            Me.UseWaitCursor = True

            GetInitialPointers()
            GenerateHeader()

            Me.Text = "Map Dumper"
        Me.UseWaitCursor = False
        Me.Enabled = True
        Me.BringToFront()

        End If

    End Sub

    Private Sub GetInitialPointers()
        MapBank = (MapsAndBanks.SelectedNode.Parent.Index)
        MapNumber = (MapsAndBanks.SelectedNode.Index)

        Point2MapBankPointers = Int32.Parse(GetString(GetINIFileLocation(), header, "Pointer2PointersToMapBanks", ""), System.Globalization.NumberStyles.HexNumber)
        MapBankPointers = ((Val(("&H" & ReverseHEX(ReadHEX(LoadedROM, Point2MapBankPointers, 4)))) - &H8000000))
        BankPointer = ((Val(("&H" & ReverseHEX(ReadHEX(LoadedROM, MapBankPointers + (MapBank * 4), 4)))) - &H8000000))
        HeaderPointer = ((Val(("&H" & ReverseHEX(ReadHEX(LoadedROM, BankPointer + (MapNumber * 4), 4)))) - &H8000000))

        If ((header2 = "BPR") Or (header2 = "BPG")) Then

            ExportName = MapNameList.Items.Item(("&H" & (ReadHEX(LoadedROM, HeaderPointer + 20, 1))) - &H58).replace(" ", "_")

        ElseIf (mMain.header2 = "BPE") Then

            ExportName = MapNameList.Items.Item("&H" & (ReadHEX(LoadedROM, HeaderPointer + 20, 1))).replace(" ", "_")

        ElseIf ((mMain.header2 = "AXP") Or (mMain.header2 = "AXV")) Then

            ExportName = MapNameList.Items.Item("&H" & (ReadHEX(LoadedROM, HeaderPointer + 20, 1))).replace(" ", "_")

        End If
    End Sub

    Private Sub GenerateHeader()
        ' OutPutHeaderText = ".align 2" & vbLf & vbLf
        OutPutHeaderText = ExportName & "_" & MapBank & "_" & MapNumber & "_Header:" & vbLf

        OutPutHeaderText = OutPutHeaderText & vbTab & ".4byte " & ExportName & "_" & MapBank & "_" & MapNumber & "_Layout" & "  @Footer" & vbLf
        OutPutHeaderText = OutPutHeaderText & vbTab & ".4byte " & "0x0" & "  @Events" & vbLf
        OutPutHeaderText = OutPutHeaderText & vbTab & ".4byte " & "0x0" & "  @Level Scripts" & vbLf
        OutPutHeaderText = OutPutHeaderText & vbTab & ".4byte " & "0x0" & "  @Connections" & vbLf

        If ((header2 = "BPR") Or (header2 = "BPG")) Then
            OutPutHeaderText = OutPutHeaderText & vbTab & ".2byte MUS_RG_MASARA" & "  @Music" & vbLf
        ElseIf (mMain.header2 = "BPE") Then
            OutPutHeaderText = OutPutHeaderText & vbTab & ".2byte " & [Enum].GetName(GetType(EM_songs), CInt("&H" & ReverseHEX(ReadHEX(LoadedROM, HeaderPointer + 16, 2)))) & "  @Music" & vbLf
        End If

        OutPutHeaderText = OutPutHeaderText & vbTab & ".2byte " & Val("&H" & ReverseHEX(ReadHEX(LoadedROM, HeaderPointer + 18, 2))) & "  @Footer ID" & vbLf

        If ((header2 = "BPR") Or (header2 = "BPG")) Then
            OutPutHeaderText = OutPutHeaderText & vbTab & ".byte " & [Enum].GetName(GetType(EM_Map_Names), (CInt("&H" & (ReadHEX(LoadedROM, HeaderPointer + 20, 1))))) & "  @Name" & vbLf
        ElseIf (mMain.header2 = "BPE") Then
            OutPutHeaderText = OutPutHeaderText & vbTab & ".byte " & [Enum].GetName(GetType(EM_Map_Names), CInt("&H" & (ReadHEX(LoadedROM, HeaderPointer + 20, 1)))) & "  @Name" & vbLf
        End If

        OutPutHeaderText = OutPutHeaderText & vbTab & ".byte " & Val(ReadHEX(LoadedROM, HeaderPointer + 21, 1)) & "  @Light" & vbLf

        OutPutHeaderText = OutPutHeaderText & vbTab & ".byte " & [Enum].GetName(GetType(EM_Weather), CInt(ReadHEX(LoadedROM, HeaderPointer + 22, 1))) & "  @Weather" & vbLf

        OutPutHeaderText = OutPutHeaderText & vbTab & ".byte " & [Enum].GetName(GetType(EM_Map_Type), CInt(ReadHEX(LoadedROM, HeaderPointer + 23, 1))) & "  @Type" & vbLf
        OutPutHeaderText = OutPutHeaderText & vbTab & ".2byte " & Val("&H" & ReverseHEX(ReadHEX(LoadedROM, HeaderPointer + 24, 2))) & "  @Can_Dig" & vbLf
        OutPutHeaderText = OutPutHeaderText & vbTab & ".byte " & Val("&H" & (ReadHEX(LoadedROM, HeaderPointer + 26, 1))) & "  @Show_Name" & vbLf
        OutPutHeaderText = OutPutHeaderText & vbTab & ".byte " & [Enum].GetName(GetType(EM_Map_Battle_Scene), CInt("&H" & (ReadHEX(LoadedROM, HeaderPointer + 27, 1)))) & "  @BattleType" & vbLf

        OutPutHeaderText = OutPutHeaderText & vbLf

        'Load Pointers

        Map_Footer = ("&H" & ReverseHEX(ReadHEX(LoadedROM, HeaderPointer, 4))) - &H8000000
        'Map_Events = ("&H" & ReverseHEX(ReadHEX(LoadedROM, HeaderPointer + 4, 4))) - &H8000000
        'Map_Level_Scripts = ("&H" & ReverseHEX(ReadHEX(LoadedROM, HeaderPointer + 8, 4))) - &H8000000
        'Map_Connection_Header = ("&H" & ReverseHEX(ReadHEX(LoadedROM, HeaderPointer + 12, 4))) - &H8000000

        'output file

        If (Not System.IO.Directory.Exists(FolderBrowserDialog1.SelectedPath & "/data/maps/" & ExportName & "_" & MapBank & "_" & MapNumber & "/")) Then
            System.IO.Directory.CreateDirectory(FolderBrowserDialog1.SelectedPath & "/data/maps/" & ExportName & "_" & MapBank & "_" & MapNumber & "/")
        End If

        File.WriteAllText(FolderBrowserDialog1.SelectedPath & "/data/maps/" & ExportName & "_" & MapBank & "_" & MapNumber & "/" & "header" & ".inc", OutPutHeaderText)

    End Sub

End Class
