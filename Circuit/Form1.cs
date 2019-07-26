using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogicCircuit;
using LogicCircuit.Nodes;

namespace Circuit
{
	public partial class Form1 : Form
	{
		// Drawing constants
		private const int OrCurvatureOffset = 10;
		private const float OrCurvatureTension = 0.8f;
		private const int XorLeftOffset = 5;
		private const int NotTipRadius = 5;

		// Data
		private LogicCircuit.Circuit _circuit = new LogicCircuit.Circuit();
		private List<NodeEntry> _nodes = new List<NodeEntry>();
		private List<WireEntry> _wires = new List<WireEntry>();

		// Editing
		private NodeEntry _selected = null;
		private int? _selectedPort;

		// Dragging
		private NodeEntry _dragging = null;
		private bool _wasDragging = false;
		private Point _dragStart = new Point();
		private Point _nodeLocationAtDragStart = new Point();

		//Drawing
		private Pen _outlinePen = new Pen(Color.Black, 3);
		private Pen _selectedOutlinePen = new Pen(Color.Cyan, 3);
		private Brush _offPortBrush = Brushes.Red;
		private Brush _onPortBrush = Brushes.Lime;
		private Brush _iconBackgroundBrush = Brushes.White;
		private int _portRadius = 15;
		private int _iconMargin = 20;

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.DoubleBuffered = true;
		}

		private void Button1_Click(object sender, EventArgs e)
		{
			Node andNode = new LogicCircuit.Nodes.AndNode();
			this.AddNode(andNode);
		}

		private void Button2_Click(object sender, EventArgs e)
		{
			Node andNode = new LogicCircuit.Nodes.OrNode();
			this.AddNode(andNode);
		}


		private void Button3_Click(object sender, EventArgs e)
		{
			Node andNode = new LogicCircuit.Nodes.XorNode();
			this.AddNode(andNode);
		}

		private void Button4_Click(object sender, EventArgs e)
		{
			Node andNode = new LogicCircuit.Nodes.NotNode();
			this.AddNode(andNode);
		}

		private void Button5_Click(object sender, EventArgs e)
		{
			Node andNode = new LogicCircuit.Nodes.SwitchNode();
			this.AddNode(andNode);
		}

		private void CircuitPanel_MouseDown(object sender, MouseEventArgs e)
		{
			this._dragStart = e.Location;
			this._dragging = this.GetNodeAtLocation(e.Location);
			if (this._dragging != null)
			{
				this._nodeLocationAtDragStart = this._dragging.Location;

				if (this._dragging.Node is InteractiveNode intN)
				{
					intN.Press();
				}
			}
		}

		private void CircuitPanel_MouseUp(object sender, MouseEventArgs e)
		{
			if (this._dragging != null && this._dragging.Node is InteractiveNode intN)
			{
				intN.Release();
			}
			this._dragging = null;
			this._wasDragging = false;
		}

		private void CircuitPanel_MouseMove(object sender, MouseEventArgs e)
		{
			if (this._dragging != null)
			{
				this._wasDragging = true;
				int deltaX = e.Location.X - this._dragStart.X;
				int deltaY = e.Location.Y - this._dragStart.Y;

				this._dragging.Location = new Point(this._nodeLocationAtDragStart.X + deltaX, this._nodeLocationAtDragStart.Y + deltaY);

				this.CircuitPanel.Refresh();
			}
		}

		private void CircuitPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (this._selectedPort != null && this._selected == null)
			{
				throw new Exception("Unexpected selection error");
			}

			if (this._dragging != null) // _dragging should be auto-filled in the MouseDown listener
			{
				if (!this._wasDragging && e.Button == MouseButtons.Left)
				{
					bool portSelected = false;
					// Check if we clicked on an input port
					for (int i = 0; i < this._dragging.Node.InputCount; i++)
					{
						var pos = this.CalculateInputPortLocation(this._dragging, i);

						if (DistanceBetween(pos.X, pos.Y, e.X, e.Y) < this._portRadius + this._outlinePen.Width)
						{
							portSelected = true;

							// There is already have a selected output port and not on the same object
							if (this._selectedPort != null && this._selectedPort == 0 && this._selected != this._dragging)
							{
								this.AddWire(new LogicCircuit.Circuit.Wire(this._selected.Node, this._dragging.Node, i + 1));

								// Clear port selection
								this._selected = null;
								this._selectedPort = null;
							}
							else // No port selected OR there was an input port selected OR we tried to connect a gate with itself => replace the selection
							{
								// Mark port selection
								this._selected = this._dragging;
								this._selectedPort = i + 1; // No current selected port => select the clicked one
							}
						}
					}

					if (!portSelected) // No port was clicked
					{
						// Check is the output port was selected
						Point outputPos = this.CalculateOutputPortLocation(this._dragging);
						if (DistanceBetween(outputPos.X, outputPos.Y, e.X, e.Y) < this._portRadius + this._outlinePen.Width) // Clicked on output port
						{
							// There was an input port selected and not on the same object => connect
							if (this._selectedPort != null && this._selectedPort > 0 && this._selected != this._dragging)
							{
								this.AddWire(new LogicCircuit.Circuit.Wire(this._dragging.Node, this._selected.Node, this._selectedPort.Value));

								// Clear port selection
								this._selected = null;
								this._selectedPort = null;
							}
							else // There was no port selected OR there was an input port selected OR we clicked on an input port but on the same object => replace the selection
							{
								this._selected = this._dragging;
								this._selectedPort = 0;
							}
						}
						else // Not clicked on output port either => simply clicked on the object somewhere
						{

						}
					}
				}
				else if(e.Button == MouseButtons.Right)
				{
					this._selected = _dragging;
					this._selectedPort = null;
				}
			}
			else // Clicked outside of any node
			{
				this._selected = null;
				this._selectedPort = null;
			}
			
			// Stop Dragging
			this._dragging = null;
			
			this.CircuitPanel.Refresh();
		}

		private void CircuitPanel_Paint(object sender, PaintEventArgs e)
		{
			this.RepaintCircuit(e.Graphics);
		}

		// Tools
		private void RepaintCircuit(Graphics g)
		{
			// High quality drawing
			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
			g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

			var halfRadius = this._portRadius / 2;

			// First Draw the wires
			foreach (var wire in this._wires)
			{
				var outputNode = this._nodes.First(n => n.Node == wire.Wire.OutputNode);
				var inputNode = this._nodes.First(n => n.Node == wire.Wire.InputNode);
				var inputIndex = wire.Wire.InputNumber - 1;

				Point from = this.CalculateOutputPortLocation(outputNode);
				Point to = this.CalculateInputPortLocation(inputNode, inputIndex);

				g.DrawLine(this._outlinePen, from, to);
			}

			// Second Draw the nodes
			foreach (var node in this._nodes)
			{
				var center = CalculateCenter(node);

				bool isNodeSelected = this._selected == node;
				bool isAnyPortSelected = this._selectedPort != null;

				// Draw Input ports
				foreach (var inputPos in this.CalculateInputPortLocations(node))
				{
					// Pick a pen based on whether this port is selected or not
					Pen inputPortOutLinePen = (this._selected == node && this._selectedPort == inputPos.Key) ? this._selectedOutlinePen : this._outlinePen;

					g.DrawLine(this._outlinePen, inputPos.Value, center);

					var inputPortRect = new RectangleF(inputPos.Value.X - halfRadius, inputPos.Value.Y - halfRadius, this._portRadius, this._portRadius);
					g.FillEllipse(this._offPortBrush, inputPortRect);
					g.DrawEllipse(inputPortOutLinePen, inputPortRect);
				}

				// Draw Output Port
				var outputPos = this.CalculateOutputPortLocation(node);
				g.DrawLine(this._outlinePen, outputPos, center);
				var outputPortRect = new RectangleF(outputPos.X - halfRadius, outputPos.Y - halfRadius, this._portRadius, this._portRadius);
				Pen outputPortOutLinePen = (this._selected == node && this._selectedPort == 0) ? this._selectedOutlinePen : this._outlinePen;
				g.FillEllipse(this._offPortBrush, outputPortRect);
				g.DrawEllipse(outputPortOutLinePen, outputPortRect);

				// Draw Icon
				var iconRect = new Rectangle(node.Location.X + this._iconMargin + this._portRadius, node.Location.Y + this._iconMargin, node.Size.Width - (this._iconMargin * 2) - (this._portRadius * 2), node.Size.Height - (this._iconMargin * 2));
				if (node.Node is AndNode) this.DrawAND(g, iconRect, node);
				else if (node.Node is OrNode) this.DrawOR(g, iconRect, node);
				else if (node.Node is XorNode) this.DrawXOR(g, iconRect, node);
				else if (node.Node is NotNode) this.DrawNOT(g, iconRect, node);
				else if (node.Node is SwitchNode) this.DrawSWITCH(g, iconRect, node);
			}
		}

		private void AddNode(Node node)
		{
			var nodeEntry = new NodeEntry(node, new Point(150, 150), new Size(100 + this._portRadius * 2, 100));

			this._nodes.Add(nodeEntry);
			this._circuit.AddNode(node);

			this.CircuitPanel.Refresh();
		}

		private void AddWire(LogicCircuit.Circuit.Wire wire)
		{
			var wireEntry = new WireEntry(wire);

			this._circuit.Connect(wire);
			this._wires.Add(wireEntry);
		}

		private NodeEntry GetNodeAtLocation(Point location)
		{
			return this._nodes.LastOrDefault(n => Intersects(location, n.Location, n.Size));
		}

		// Helpers
		private IEnumerable<KeyValuePair<int, Point>> CalculateInputPortLocations(NodeEntry entry)
		{
			for (int i = 0; i< entry.Node.InputCount; i++)
			{
				yield return new KeyValuePair<int, Point>(i + 1, CalculateInputPortLocation(entry, i));
			}
		}

		private Point CalculateInputPortLocation(NodeEntry entry, int portIndex)
		{
			var portPartHeight = (entry.Size.Height - this._portRadius * 2) / entry.Node.InputCount;
			var portPartHalf = portPartHeight / 2;

			return new Point(entry.Location.X + this._portRadius, entry.Location.Y + this._portRadius + (portIndex * portPartHeight) + portPartHalf);
		}

		private Point CalculateOutputPortLocation(NodeEntry entry)
		{
			var heightHalf = entry.Size.Height / 2;

			return new Point(entry.Location.X + entry.Size.Width - this._portRadius, entry.Location.Y + heightHalf);
		}

		private Point CalculateCenter(NodeEntry entry)
		{
			return new Point(entry.Location.X + (entry.Size.Width / 2), entry.Location.Y + (entry.Size.Height / 2));
		}

		private void DrawAND(Graphics g, Rectangle backgroundRect, NodeEntry entry)
		{
			var hX = backgroundRect.Width / 2;
			var cX = backgroundRect.X + hX;

			GraphicsPath path = new GraphicsPath();
			path.AddArc(cX, backgroundRect.Top, hX, backgroundRect.Height, 270, 180);
			path.AddLine(new Point(backgroundRect.Left, backgroundRect.Bottom), new Point(cX, backgroundRect.Bottom)); // Bottom
			path.AddLine(new Point(backgroundRect.Left, backgroundRect.Bottom), new Point(backgroundRect.Left, backgroundRect.Top)); // Left
			path.AddLine(new Point(cX, backgroundRect.Top), new Point(backgroundRect.Left, backgroundRect.Top)); // Top
			path.CloseFigure();

			g.FillPath(this._iconBackgroundBrush, path);
			g.DrawPath(this._outlinePen, path);
		}

		private void DrawOR(Graphics g, Rectangle backgroundRect, NodeEntry entry)
		{
			int cX = backgroundRect.X + (backgroundRect.Width / 2);
			int cY = backgroundRect.Y + (backgroundRect.Height / 2);

			// Draw enough curves so that it can fully connect with the rectangle
			GraphicsPath path = new GraphicsPath();
			path.AddCurve(new Point[] // Left Curve
			{
				new Point(backgroundRect.Left, backgroundRect.Top),
				new Point(backgroundRect.Left + OrCurvatureOffset, cY),
				new Point(backgroundRect.Left, backgroundRect.Bottom),
			}, OrCurvatureTension);

			path.AddLine(new Point(backgroundRect.Left, backgroundRect.Bottom), new Point(cX, backgroundRect.Bottom)); // Bottom Line
			path.AddCurve(new Point[] // Right bottom curve
			{
				new Point(cX, backgroundRect.Bottom),
				new Point(cX + OrCurvatureOffset, backgroundRect.Bottom - (OrCurvatureOffset / 2)),

				new Point(backgroundRect.Right - OrCurvatureOffset, cY + (OrCurvatureOffset / 2)),
				new Point(backgroundRect.Right, cY),
			}, OrCurvatureTension);
			path.AddCurve(new Point[] // Right Top curve
			{
				new Point(backgroundRect.Right, cY),
				new Point(backgroundRect.Right - OrCurvatureOffset, cY - (OrCurvatureOffset / 2)),

				new Point(cX + OrCurvatureOffset, backgroundRect.Top + (OrCurvatureOffset / 2)),
				new Point(cX, backgroundRect.Top ),
			}, OrCurvatureTension);

			//path.AddLine(new Point(centerX, centerY - halfSide), new Point(centerX - halfSide, centerY - halfSide)); // Top
			path.CloseFigure();

			g.FillPath(this._iconBackgroundBrush, path);
			g.DrawPath(this._outlinePen, path);
		}

		private void DrawXOR(Graphics g, Rectangle backgroundRect, NodeEntry entry)
		{
			int cY = backgroundRect.Y + (backgroundRect.Height / 2);

			this.DrawOR(g, new Rectangle(backgroundRect.X + XorLeftOffset, backgroundRect.Y, backgroundRect.Width - XorLeftOffset, backgroundRect.Height), entry);
			g.DrawCurve(this._outlinePen, new Point[]
			{
				new Point(backgroundRect.Left, backgroundRect.Top),
				new Point(backgroundRect.Left + OrCurvatureOffset, cY),
				new Point(backgroundRect.Left, backgroundRect.Bottom),
			}, OrCurvatureTension);
		}

		private void DrawNOT(Graphics g, Rectangle backgroundRect, NodeEntry entry)
		{
			var cY = backgroundRect.Y + (backgroundRect.Height / 2);

			GraphicsPath path = new GraphicsPath();
			path.AddLine(new Point(backgroundRect.Left, backgroundRect.Top), new Point(backgroundRect.Left, backgroundRect.Bottom)); // Rear Line
			path.AddLine(new Point(backgroundRect.Left, backgroundRect.Bottom), new Point(backgroundRect.Right - NotTipRadius * 2, cY));
			path.CloseFigure();

			path.AddEllipse(new Rectangle(new Point(backgroundRect.Right - NotTipRadius * 2, cY - NotTipRadius), new Size(NotTipRadius * 2, NotTipRadius * 2)));

			g.FillPath(this._iconBackgroundBrush, path);
			g.DrawPath(this._outlinePen, path);
		}

		private void DrawSWITCH(Graphics g, Rectangle backgroundRect, NodeEntry entry)
		{
			g.FillRectangle(this._iconBackgroundBrush, backgroundRect);
			g.DrawRectangle(this._outlinePen, backgroundRect);
		}

		private bool Intersects(Point reference, Point offset, Size size)
		{
			return (reference.X >= offset.X && reference.X <= offset.X + size.Width)
				&& (reference.Y >= offset.Y && reference.Y <= offset.Y + size.Height);
		}

		private double DistanceBetween(int x1, int y1, int x2, int y2)
		{
			var a = x1 - x2;
			var b = y1 - y2;
			return Math.Sqrt((a * a) + (b * b));
		}
	}

	class NodeEntry
	{
		public Node Node { get; set; }

		public Point Location { get; set; }

		public Size Size { get; set; }

		public NodeEntry(Node node, Point location, Size size)
		{
			this.Node = node;
			this.Location = location;
			this.Size = size;
		}
	}

	class WireEntry
	{
		public LogicCircuit.Circuit.Wire Wire { get; }

		public WireEntry(LogicCircuit.Circuit.Wire wire)
		{
			this.Wire = wire;
		}
	}
}
