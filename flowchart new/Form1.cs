using Dataweb.NShape;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dataweb.NShape.Advanced;
using System.Diagnostics;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.Layouters;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;


namespace flowchart_new
{
    public partial class Form1 : Form
    {
        List<RectangleBase> shapes = new List<RectangleBase>();

        //member to save all connected lines
        List<Polyline> lines = new List<Polyline>();

        List<Unit> units = new List<Unit>();

        Process gdb;
        bool gdbStarted = false;

        string watchText = "";
       
        int elseDistance = 0;

        public Form1()
        {
            InitializeComponent();
        }

      

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                //initiate project
                project1.Name = "FlowChart";
                project1.AddLibraryByName("Dataweb.NShape.GeneralShapes", true);
                project1.AddLibraryByName("Dataweb.NShape.FlowChartShapes", true);

                //auto load shape librarys - no need in here
                project1.AutoLoadLibraries = true;

                //create new flowchart project
                project1.Create();

                //create diagram for drawing flowchart
                Diagram diagram = new Diagram("flowchart");
                diagram.Width = 600;
                diagram.Height = 800;

                //set diagram to display1
                display1.Diagram = diagram;
                display1.ZoomLevel = 55;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void outputReceived(object sender, DataReceivedEventArgs args)
        {
            watchText += args.Data + "\n";

        }

        private void parseRecordsToDiagram(List<Record> records)
        {
            // initiate draw step coordinates
            int x = 0;
            int y = 10;

            //max x, y
            int maxX = 0;
            int maxY = 0;

            //padding for up-down blocks
            int rowPadding = 80;

            //creates start block
            RectangleBase shape = (RectangleBase)project1.ShapeTypes["Terminator"].CreateInstance();
            shape.Text = "Start";
            shape.Width = 200;
            shape.Height = 60;
            shape.X = x;
            shape.Y = y;

            //grows positon for block
            maxX = Math.Abs(x) < maxX ? maxX : x;
            y += shape.Height + rowPadding;

            //add start block to shapes
            shapes.Add(shape);


            //add block to diagram
            display1.Diagram.Shapes.Add(shape);

            //parse all records to drawing flowchart
            int findedShapeIndex;

            //for each record
            foreach (Record record in records)
            {
                this.elseDistance++;
                //parse IF ELSEIF clause
                if (record.operation == "IF" || record.operation == "ELSEIF")
                {
                    //create Diamond block
                    shape = (RectangleBase)project1.ShapeTypes["Diamond"].CreateInstance();

                    //set position
                    shape.Width = 160;
                    shape.Height = 120;
                    shape.Y = y;
                    shape.X = 0;
                    y += shape.Height + rowPadding;



                    //connects line direction to above block
                    generateLine(shapes[shapes.Count - 1], shape, "", record);

                    //add to shapes
                    shapes.Add(shape);
                    units.Add(new Unit(shape, record));
                    display1.Diagram.Shapes.Add(shape);
                }

                //parse THEN ELSE record
                if (record.operation == "THEN" || record.operation == "ELSE")
                {
                    if (record.operation == "ELSE")
                    {
                        this.elseDistance = 0;
                    }
                    //creates Process block
                    shape = (RectangleBase)project1.ShapeTypes["RoundedBox"].CreateInstance();

                    //set width and height
                    shape.Width = 350;
                    shape.Height = 120;

                    //set positon of process block
                    x = record.operation == "THEN" ? -400 : 400;
                    maxX = Math.Abs(x) < maxX ? maxX : Math.Abs(x);
                    shape.X = x;
                    y = record.operation == "THEN" ? y : y - shape.Height - rowPadding;
                    shape.Y = y;
                    y += shape.Height + rowPadding;

                    //finds above diamond block
                    findedShapeIndex = shapes.Count - 1;
                    for (int i = findedShapeIndex; i >= 0; i--)
                    {
                        RectangleBase tempShape = shapes[i];
                        if (tempShape.Type.Name == "Diamond")
                        {
                            findedShapeIndex = i;
                            break;
                        }
                    }
                    //connects line direction to above block
                    generateLine(shapes[findedShapeIndex], shape, record.operation, record);

                    shapes.Add(shape);
                    units.Add(new Unit(shape, record));

                    display1.Diagram.Shapes.Add(shape);
                }
                else if (record.output != null && record.output != "" &&
                    shapes[shapes.Count - 1].Type.Name != "RoundedBox")
                {
                    //create new RoundedBox block
                    shape = (RectangleBase)project1.ShapeTypes["RoundedBox"].CreateInstance();
                    shape.Width = 350;
                    shape.Height = 120;
                    shape.X = x;
                    shape.Y = y;
                    y += shape.Height + rowPadding;

                    //connects line direction to above block
                    generateLine(shapes[shapes.Count - 1], shape, "", record);
                    shapes.Add(shape);
                    units.Add(new Unit(shape, record));
                    display1.Diagram.Shapes.Add(shape);
                }

                //seperate no more than 4 lines
                if (shapes[shapes.Count - 1].Type.Name == "RoundedBox")
                {
                    string text = ((RectangleBase)shapes[shapes.Count - 1]).Text;
                    int lineCount = text.Split('\n').Length;
                    if (lineCount > 4 || this.elseDistance == 2)//number of lines > 4
                    {
                        //create new process block
                        shape = (RectangleBase)project1.ShapeTypes["RoundedBox"].CreateInstance();
                        shape.Width = 350;
                        shape.Height = 120;
                        x = 0;
                        shape.X = x;
                        shape.Y = y;
                        y += shape.Height + rowPadding;

                        //coonnects line directin to above block
                        generateLine(shapes[shapes.Count - 1], shape, "", record);
                        shapes.Add(shape);
                        units.Add(new Unit(shape, record));
                        display1.Diagram.Shapes.Add(shape);

                        //finds above Diamond block
                        findedShapeIndex = shapes.Count - 1;
                        for (int i = findedShapeIndex; i >= 0; i--)
                        {
                            RectangleBase tempShape = shapes[i];
                            if (tempShape.Type.Name == "Diamond")
                            {
                                findedShapeIndex = i;
                                break;
                            }
                        }
                        //connects line direction to above case block
                        generateLine(shapes[findedShapeIndex + 1], shape, "", record);
                    }
                }

                //parse  >,<,>=,<=,== records
                if (record.operation == ">" ||
                    record.operation == "<" ||
                    record.operation == ">=" ||
                    record.operation == "<=" ||
                    record.operation == "==")
                {
                    //constructs string for display in diamond
                    shape = shapes[shapes.Count - 1];
                    string text = record.operands.Replace(",", " " + record.operation + " ") + "\n";
                    shape.Text = text;
                }
                //parse mathematical operations
                else if ((record.operation == "+" ||
                    record.operation == "-" ||
                    record.operation == "*" ||
                    record.operation == "/" ||
                    record.operation == "=" ||
                    record.operation == "AND" ||
                    record.operation == "OR" ||
                    record.operation == "NOT" ||
                    record.operation == "XOR") && record.output != null && record.output != "")
                {
                    //constructs string for display in process block
                    shape = shapes[shapes.Count - 1];
                    string text = record.output + " = " + record.operands.Replace(",", " " + record.operation + " ") + "\n";
                    shape.Text += text;

                    units.ForEach((unit) =>
                    {
                        if (unit.shape.Equals(shape))
                        {
                            if (unit.records.Count == 0 || !unit.records.ToArray()[unit.records.Count - 1].Equals(record))
                            {
                                unit.records.Add(record);
                            }
                        }
                    });
                }
            }

            //creates end shape
            shape = (RectangleBase)project1.ShapeTypes["Terminator"].CreateInstance();
            shape.Text = "End";
            shape.Width = 200;
            shape.Height = 60;
            shape.X = 0;
            shape.Y = y;
            y += shape.Height + rowPadding;

            //find above diamond block
            findedShapeIndex = shapes.Count - 1;
            for (int i = findedShapeIndex; i >= 0; i--)
            {
                RectangleBase tempShape = shapes[i];
                if (tempShape.Type.Name == "Diamond")
                {
                    findedShapeIndex = i;
                    break;
                }
            }

            //connects line directin to above diamond block
            if (findedShapeIndex < shapes.Count - 1)
            {
                generateLine(shapes[findedShapeIndex + 1], shape);
            }
            //connects line directin
            generateLine(shapes[shapes.Count - 1], shape);
            shapes.Add(shape);
            display1.Diagram.Shapes.Add(shape);

            //set max x,y
            maxY = y + 100;
            maxX = maxX + 250;

            //determines width and height of diagram
            display1.Diagram.Width = 2 * maxX;
            display1.Diagram.Height = maxY;


            //transforms all block's coordinates
            int addX = maxX;
            int addY = 30;
            foreach (Shape shapeOne in shapes)
            {
                shapeOne.X += addX;
                shapeOne.Y += addY;
            }

            //saves diagram to repository
            cachedRepository1.InsertAll(display1.Diagram);
        }

        //functin to connect blocks each other
        private void generateLine(ShapeBase firstShape, ShapeBase lastShape, string str = "", Record r = null)
        {
            //creates a line
            Polyline arrow = (Polyline)project1.ShapeTypes["Polyline"].CreateInstance();

            //set line color - blue-yes, red-no
            if (str == "THEN")
            {
                arrow.LineStyle = project1.Design.LineStyles.Blue;
            }
            else if (str == "ELSE" || str == "ELSEIF")
            {
                arrow.LineStyle = project1.Design.LineStyles.Red;
            }
            arrow.EndCapStyle = project1.Design.CapStyles.ClosedArrow;
            // Connect one of the line shape's endings (first vertex) to the referring shape's reference point
            arrow.Connect(ControlPointId.FirstVertex, firstShape, ControlPointId.Reference);
            // Connect the other of the line shape's endings (last vertex) to the referred shape
            arrow.Connect(ControlPointId.LastVertex, lastShape, ControlPointId.Reference);
            units.Add(new Unit(arrow, r));
            lines.Add(arrow);
            display1.Diagram.Shapes.Add(arrow);
        }

        private void OnLoadExcelFile(object sender, EventArgs e)
        {
            
			ExcelManager excelManager = new ExcelManager();
			excelManager.readFromExcelFile("e:\\sample_debugger.xlsx");
			if (excelManager.allRecords.Count > 0)
			{
				//parse all records
				parseRecordsToDiagram(excelManager.allRecords);
			}
			
          /*  var filePath = string.Empty;
            //opens file dialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "excel file (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    //creates excel manager to read excel file
                    ExcelManager excelManager = new ExcelManager();
                    excelManager.readFromExcelFile(filePath);
                    if (excelManager.allRecords.Count > 0)
                    {
                        //parse all records
                        parseRecordsToDiagram(excelManager.allRecords);
                    }
                }
            }
            */

        }


        private void setBreakpoint(Object sender, System.EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Tag tag = (Tag)item.Tag;

            if (tag.record == null)
            {
                return;
            }
            bool result = runGdbCommand("break " + tag.record.line);

            if (!result)
            {
                return;
            }

            if (tag.shape.Type.Name != "Polyline")
            {
                ((RectangleBase)tag.shape).Tag = ((RectangleBase)tag.shape).FillStyle;
                ((RectangleBase)tag.shape).FillStyle = project1.Design.FillStyles.Red;
            }
            else
            {
                ((Polyline)tag.shape).Tag = ((Polyline)tag.shape).LineStyle;
                ((Polyline)tag.shape).LineStyle = project1.Design.LineStyles.HighlightThick;
            }

        }
    
        private void removeBreakpoint(Object sender, System.EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Tag tag = (Tag)item.Tag;

            bool result = runGdbCommand("delete " + tag.record.line);

            if (!result)
            {
                return;
            }

            if (tag.shape.Type.Name != "Polyline")
            {
                if (((RectangleBase)tag.shape).Tag != null)
                {
                    ((RectangleBase)tag.shape).FillStyle = (IFillStyle)((RectangleBase)tag.shape).Tag;
                }
                ((RectangleBase)tag.shape).Tag = null;
            }
            else
            {
                if (((Polyline)tag.shape).Tag != null)
                {
                    ((Polyline)tag.shape).LineStyle = (ILineStyle)((Polyline)tag.shape).Tag;
                }
                ((Polyline)tag.shape).Tag = null;

            }
        }
        private void trace2code(Object sender, System.EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Tag tag = (Tag)item.Tag;
            HighlightLine(this.codeTextBox, tag.record.line - 1, System.Drawing.Color.Yellow);
        }
        public static void HighlightLine(RichTextBox richTextBox, int index, System.Drawing.Color color)
        {
            richTextBox.SelectAll();                 
            richTextBox.SelectionBackColor = richTextBox.BackColor;
            var lines = richTextBox.Lines;
            if (index < 0 || index >= lines.Length)
                return;
            var start = richTextBox.GetFirstCharIndexFromLine(index);  // Get the 1st char index of the appended text
            var length = lines[index].Length;
            richTextBox.Select(start, length);                 // Select from there to the end
            richTextBox.SelectionBackColor = color;
        }
        private void runUpToHere(Object sender, System.EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Tag tag = (Tag)item.Tag;

            runGdbCommand("until " + tag.record.line);
        }
        private void PaneClick(object sender, EventArgs e)
        {
            this.display1.ContextMenuStrip.Items.Clear();
        }
        private void sourcedWhere(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Record record = ((Tag)item.Tag).record;

            this.codeTextBox.SelectionStart = 0;
            this.codeTextBox.SelectAll();
            this.codeTextBox.SelectionBackColor = Color.White;

            string word = record.output;
            int startindex = 0;
            while (startindex < this.codeTextBox.TextLength)
            {
                int wordstartIndex = this.codeTextBox.Find(word + " = ", startindex, RichTextBoxFinds.None);
                if (wordstartIndex != -1)
                {
                    this.codeTextBox.SelectionStart = wordstartIndex;
                    this.codeTextBox.SelectionLength = word.Length; 
                    this.codeTextBox.SelectionBackColor = Color.Yellow;
                }
                else
                    break;
                startindex = wordstartIndex + word.Length;
            }
        }
        private void usedWhere(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Record record = ((Tag)item.Tag).record;

            this.codeTextBox.SelectionStart = 0;
            this.codeTextBox.SelectAll();
            this.codeTextBox.SelectionBackColor = Color.White;

            string word = record.output;
            List<int> indexList = new List<int>();
            int startindex = 0;
            while (startindex < this.codeTextBox.TextLength)
            {
                int wordstartIndex = this.codeTextBox.Find(word + " = ", startindex, RichTextBoxFinds.WholeWord);
                if (wordstartIndex > 0 && !indexList.Contains(wordstartIndex))
                {
                    indexList.Add(wordstartIndex);
                }
                if (wordstartIndex < 0)
                {
                    break;
                }
                startindex = wordstartIndex + word.Length;
            }

            startindex = 0;
            while (startindex < this.codeTextBox.TextLength)
            {
                int wordstartIndex = this.codeTextBox.Find(word, startindex, RichTextBoxFinds.WholeWord);

                if (wordstartIndex == -1)
                {
                    startindex++;
                }
                else if (!indexList.Contains(wordstartIndex))
                {
                    this.codeTextBox.SelectionStart = wordstartIndex;
                    this.codeTextBox.SelectionLength = word.Length;
                    this.codeTextBox.SelectionBackColor = Color.Yellow;
                    startindex += wordstartIndex + word.Length;
                }
                else
                {
                    startindex = wordstartIndex + word.Length;
                }
            }
        }
        private void watch(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            Record record = ((Tag)item.Tag).record;
            runGdbCommand("watch " + record.output);
          
   
        }
        private void display1_ShapeClick(object sender, Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs e)
        {
            this.display1.ContextMenuStrip.Items.Clear();
            if (e.Shape.Type.Name == "Polyline"
                || e.Shape.Type.Name == "Diamond"
                || e.Shape.Type.Name == "RoundedBox"
                || e.Shape.Type.Name == "Terminator")
            {
                Record breakRecord = null;
                units.ForEach((unit) =>
                {
                    if (unit.shape.Equals(e.Shape))
                    {
                        if (unit.records.Count > 0)
                        {
                            breakRecord = unit.records.ToArray()[0];
                        }
                    }
                });
                ToolStripMenuItem item = new ToolStripMenuItem("set breakpoint", null, new EventHandler(setBreakpoint));
                item.Tag = new Tag(e.Shape, breakRecord);
                this.display1.ContextMenuStrip.Items.Add(item);
                item = new ToolStripMenuItem("Remove breakpoint", null, new EventHandler(removeBreakpoint));
                item.Tag = new Tag(e.Shape, breakRecord);
                this.display1.ContextMenuStrip.Items.Add(item);
                item = new ToolStripMenuItem("trace to code", null, new EventHandler(trace2code));
                item.Tag = new Tag(e.Shape, breakRecord);
                this.display1.ContextMenuStrip.Items.Add(item);
                item = new ToolStripMenuItem("run Up to here", null, new EventHandler(runUpToHere));
                item.Tag = new Tag(e.Shape, breakRecord);
                this.display1.ContextMenuStrip.Items.Add(item);

                if (e.Shape.Type.Name == "RoundedBox")
                {
                    units.ForEach((unit) =>
                    {
                        if (unit.shape.Equals(e.Shape))
                        {
                            //add sub items
                            unit.records.ForEach((r) =>
                            {
                                if (r.output != null && r.operands != null && r.operation != null)
                                {
                                    string text = r.output + " = " + r.operands.Replace(",", " " + r.operation + " ");
                                    item = new ToolStripMenuItem(text);
                                    ToolStripMenuItem subItem = new ToolStripMenuItem("Set breakpoint", null, new EventHandler(setBreakpoint));
                                    subItem.Tag = new Tag(e.Shape, r);
                                    item.DropDownItems.Add(subItem);
                                    subItem = new ToolStripMenuItem("Remove breakpoint", null, new EventHandler(removeBreakpoint));
                                    subItem.Tag = new Tag(e.Shape, r);
                                    item.DropDownItems.Add(subItem);
                                    subItem = new ToolStripMenuItem("trace to code", null, new EventHandler(trace2code));
                                    subItem.Tag = new Tag(e.Shape, r);
                                    item.DropDownItems.Add(subItem);
                                    subItem = new ToolStripMenuItem("run Up to here", null, new EventHandler(runUpToHere));
                                    subItem.Tag = new Tag(e.Shape, r);
                                    item.DropDownItems.Add(subItem);
                                    subItem = new ToolStripMenuItem("sourced where", null, new EventHandler(sourcedWhere));
                                    subItem.Tag = new Tag(e.Shape, r);
                                    item.DropDownItems.Add(subItem);
                                    subItem = new ToolStripMenuItem("used where", null, new EventHandler(usedWhere));
                                    subItem.Tag = new Tag(e.Shape, r);
                                    item.DropDownItems.Add(subItem);
                                    subItem = new ToolStripMenuItem("watch", null, new EventHandler(watch));
                                    subItem.Tag = new Tag(e.Shape, r);
                                    item.DropDownItems.Add(subItem);
                                    this.display1.ContextMenuStrip.Items.Add(item);
                                }
                            });
                        }
                    });
                }
            }
        }
        private bool runGdbCommand(string command)
        {
            if (!gdbStarted)
            {
                MessageBox.Show("GDB wasn't started yet!");
                return false;
            }
            watchText = "";
            gdb.StandardInput.WriteLine(command);
            gdb.StandardInput.Flush();

            Thread.Sleep(200);
            this.watchTextBox.Clear();
            this.watchTextBox.Text = watchText;
            
            highlightRun(watchText);
            return true;
        }
        private void highlightRun(string log)
        {
            shapes.ForEach((shape) =>
            {
                if (shape.FillStyle.Equals(project1.Design.FillStyles.Yellow))
                {
                    //if(shape.Tag != null)
                    //{
                    shape.FillStyle = project1.Design.FillStyles.Blue;
                    //	shape.Tag = null;
                    //}

                }
            });
            this.lines.ForEach((line) =>
            {
                if (line.LineStyle.Equals(project1.Design.LineStyles.HighlightThick))
                {
                    if (line.Tag != null)
                    {
                        line.LineStyle = (ILineStyle)line.Tag;
                        line.Tag = null;
                    }
                }
            });
            if (log.Contains("Starting program:"))
            {
                var startShape = shapes.ToArray()[0];
                ((RectangleBase)startShape).Tag = ((RectangleBase)startShape).FillStyle;
                ((RectangleBase)startShape).FillStyle = project1.Design.FillStyles.Yellow;
            }
            else if (log.Contains("exited normally]"))
            {
                var endShape = shapes.ToArray()[shapes.Count - 1];
                ((RectangleBase)endShape).Tag = ((RectangleBase)endShape).FillStyle;
                ((RectangleBase)endShape).FillStyle = project1.Design.FillStyles.Yellow;
            }
            string[] lines = log.Split(new char[] { '\n' });
            if (lines.Length > 1)
            {
                string line = lines[lines.Length - 2];
                if (line.IndexOf('\t') >= 0)
                {
                    string numstr = line.Substring(0, line.IndexOf('\t'));
                    numstr = numstr.Replace("(gdb)", "");
                    if (Int32.TryParse(numstr, out int lineNum))
                    {
                        units.ForEach((unit) =>
                        {
                            unit.records.ForEach((r) =>
                            {
                                if (r != null && r.line == lineNum)
                                {
                                    //highlight
                                    if (unit.shape.Type.Name != "Polyline")
                                    {
                                        ((RectangleBase)unit.shape).Tag = ((RectangleBase)unit.shape).FillStyle;
                                        ((RectangleBase)unit.shape).FillStyle = project1.Design.FillStyles.Yellow;
                                    }
                                    else
                                    {
                                        ((Polyline)unit.shape).Tag = ((Polyline)unit.shape).LineStyle;
                                        ((Polyline)unit.shape).LineStyle = project1.Design.LineStyles.HighlightThick;
                                    }
                                }
                            });
                        });
                    }
                }
            }


        }
        private void play(object sender, EventArgs e)
        {
            watchText = "";
            gdb = new Process();
            gdb.StartInfo.FileName = "mingw\\bin\\gdb";
            gdb.StartInfo.Arguments = "work\\code.exe";
            gdb.StartInfo.RedirectStandardInput = true;
            gdb.StartInfo.RedirectStandardOutput = true;
            gdb.StartInfo.CreateNoWindow = true;
            gdb.StartInfo.UseShellExecute = false;

            gdb.OutputDataReceived += outputReceived;

            gdb.Refresh();  // Important

            gdb.Start();
            gdbStarted = true;
            gdb.BeginOutputReadLine();

            runGdbCommand("start");
        }

        private void stop(object sender, EventArgs e)
        {
            gdb.Close();
            gdbStarted = false;
            this.watchTextBox.Text += "\n stopped";
        }

        private void stepInto(object sender, EventArgs e)
        {
            runGdbCommand("step");
        }

        private void stepNext(object sender, EventArgs e)
        {
            runGdbCommand("next");
        }

        private void removeBreakpoints(object sender, EventArgs e)
        {
            runGdbCommand("delete");
        }
        private void compile(object sender, EventArgs e)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine("mingw\\bin\\gcc -g work\\code.c -o work\\code.exe");
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.WaitForExit();

            this.watchTextBox.Text = cmd.StandardOutput.ReadToEnd();

        }
        private void loadCFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            Directory.CreateDirectory("work");

            String text = File.ReadAllText("e:\\sample_debugger.c");

            File.WriteAllText("work\\code.c", text);

            this.codeTextBox.Text = text;
            */
            var filePath = string.Empty;
            //opens file dialog
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "c file (*.c)|*.c|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Directory.CreateDirectory("work");

                    //Get the path of specified file
                    filePath = openFileDialog.FileName;

                    String text = File.ReadAllText(filePath);

                    File.WriteAllText("work\\code.c", text);

                    this.codeTextBox.Text = text;
                }
            }

        }

        private void ContinueOrStart(object sender, EventArgs e)
        {
            runGdbCommand("continue");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Reset the results box.
           
            string[] data = new string[2];

            // Get the word to search from from TextBox2.
            string[] searchWord = new string[] { "Old","New" };
            
            int index = 0;
            int i = 0;
            //Declare an ArrayList to store line numbers.
            System.Collections.ArrayList lineList = new System.Collections.ArrayList();
            foreach(string s in searchWord)
                
            
            {
                // Find occurrences of the search word, incrementing  
                // the start index. 
                index = watchTextBox.Find(s , index + 1, RichTextBoxFinds.MatchCase);
                if (index != -1)

                // Find the word's line number and add the line 
                // number to the arrayList. 
                {

                    
                    data[i] =      watchTextBox.Lines[watchTextBox.GetLineFromCharIndex(index)] ;
                       /* textBox1.Text = data[i];*/
                        index = 0;
                    i++;
                    
                    
                }

                
            }
            Class1.temparray = data;

            Form2 form2 = new Form2();
            
            form2.Show();
            
        } 

       
    }
}
