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
		private const int WireSelectMaxDistance = 10;

		// Data
		private LogicCircuit.Circuit _circuit = new LogicCircuit.Circuit();
		private List<NodeEntry> _nodes = new List<NodeEntry>();
		private List<WireEntry> _wires = new List<WireEntry>();

		// Editing
		private NodeEntry _selected = null;
		private int? _selectedPort;
		private WireEntry _selectedWire = null;

		// Dragging
		private NodeEntry _interacted = null;
		private bool _startedDragging = false;
		private bool _didDragging = false;
		private bool _isPressed = false;
		private Point _dragStart = new Point();
		private Point _nodeLocationAtDragStart = new Point();

		//Drawing
		private Pen _outlinePen = new Pen(Color.Black, 3);
		private Pen _selectedOutlinePen = new Pen(Color.Cyan, 3);
		private Brush _offBrush = Brushes.Red;
		private Brush _onBrush = Brushes.Lime;
		private Brush _iconBackgroundBrush = Brushes.White;
		private int _portRadius = 15;
		private int _iconMargin = 20;
		private float _interactiveZoneRatio = 0.3f;
		private int _portsMargin = 15;
		private Font _gateNameFont = new Font("Arial", 12, FontStyle.Bold);

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			this.DoubleBuffered = true;

			this._circuit.OnUpdate += this.OnCircuitUpdate;
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

		private void Button6_Click(object sender, EventArgs e)
		{
			this.AddNode(new LogicCircuit.Nodes.BulbNode());
		}

		private void CircuitPanel_MouseDown(object sender, MouseEventArgs e)
		{
			this._interacted = this.GetNodeAtLocation(e.Location);
			if (this._interacted != null && e.Button == MouseButtons.Left)
			{
				this._nodeLocationAtDragStart = this._interacted.Location;

				if (this._interacted.IsInteractive && this.CalculateInteractiveZone(this.CalculateIconRect(this._interacted.Rect, this._interacted.InputOnly)).Contains(e.Location))
				{
					this._isPressed = true;
					((InteractiveNode)this._interacted.Node).Press();
				}
				else
				{
					this._dragStart = e.Location;
					this._startedDragging = true;
				}
			}
		}

		private void CircuitPanel_MouseUp(object sender, MouseEventArgs e)
		{
			if (this._interacted != null && this._interacted.Node is InteractiveNode intN && this._isPressed)
			{
				intN.Release();
			}
			this._interacted = null;
			this._isPressed = false;
			this._startedDragging = false;
			this._didDragging = false;
		}

		private void CircuitPanel_MouseMove(object sender, MouseEventArgs e)
		{
			if (this._interacted != null && !this._isPressed && this._startedDragging)
			{
				this._didDragging = true;
				int deltaX = e.Location.X - this._dragStart.X;
				int deltaY = e.Location.Y - this._dragStart.Y;

				this._interacted.Location = new Point(this._nodeLocationAtDragStart.X + deltaX, this._nodeLocationAtDragStart.Y + deltaY);

				this.CircuitPanel.Refresh();
			}
		}

		private void CircuitPanel_MouseClick(object sender, MouseEventArgs e)
		{
			if (this._selectedPort != null && this._selected == null)
			{
				throw new Exception("Unexpected selection error");
			}

			if (this._didDragging)
			{
				return;
			}


			if (e.Button == MouseButtons.Left)
			{
				if (this._interacted != null) // _dragging should be auto-filled in the MouseDown listener, if it's null => the user hasn't clicked on a node
				{
					if (this.IsNodeInputPortClicked(this._interacted, e.Location, out int portNumber)) // Check if we clicked on an input port
					{
						// There is already have a selected output port and not on the same object
						if (this._selectedPort != null && this._selectedPort == 0 && this._selected != this._interacted)
						{
							this.AddWire(new LogicCircuit.Circuit.Wire(this._selected.Node, this._interacted.Node, portNumber));

							// Clear port selection
							this._selected = null;
							this._selectedPort = null;
						}
						else // No port selected OR there was an input port selected OR we tried to connect a gate with itself => replace the selection
						{
							// Mark port selection
							this._selected = this._interacted;
							this._selectedPort = portNumber; // No current selected port => select the clicked one
						}
					}
					else if (IsNodeOutputPortClicked(this._interacted, e.Location)) // No input port was selected => Check is the output port was selected
					{
						// There was an input port selected and not on the same object => connect
						if (this._selectedPort != null && this._selectedPort > 0 && this._selected != this._interacted)
						{
							this.AddWire(new LogicCircuit.Circuit.Wire(this._interacted.Node, this._selected.Node, this._selectedPort.Value));

							// Clear port selection
							this._selected = null;
							this._selectedPort = null;
						}
						else // There was no port selected OR there was an input port selected OR we clicked on an input port but on the same object => replace the selection
						{
							this._selected = this._interacted;
							this._selectedPort = 0;
						}
					}
					else // Not clicked on output port either => simply clicked on the object somewhere
					{
						
					}
				}
				else // Clicked outside of a node
				{
					this._selected = null;
					this._selectedPort = null;
				}
			}
			else if (e.Button == MouseButtons.Right)
			{
				WireEntry wireEntry;
				if (this._interacted != null) // Clicked on node
				{
					this._selected = _interacted;
					this._selectedPort = null;
					this.nodeRightClickMenu.Show(this.CircuitPanel.PointToScreen(e.Location));
				}
				else if ((wireEntry = GetWireAt(e.Location)) != null) // Clicked near wire
				{
					this._selectedWire = wireEntry;
					wireRightClickMenu.Show(this.CircuitPanel.PointToScreen(e.Location));
				}
				else
				{
					this._selected = null;
					this._selectedWire = null;
					this._selectedPort = null;
				}
			}

			// Stop Dragging
			this._interacted = null;

			this.CircuitPanel.Refresh();
		}

		private void DeleteToolStripMenuItem_Click(object sender, EventArgs e) // Wire
		{
			if (this._selectedWire != null)
			{
				this._circuit.Disconnect(this._selectedWire.Wire);
				this._wires.Remove(this._selectedWire);
				this._selectedWire = null;
				this.CircuitPanel.Refresh();
			}	
		}

		private void DeleteToolStripMenuItem1_Click(object sender, EventArgs e) // Node
		{
			if (this._selected != null && this._selectedPort == null)
			{
				this._circuit.RemoveNode(this._selected.Node);
				// Delete all wire
				var deletedWires = this._wires.Where(w => w.Wire.InputNode == this._selected.Node || w.Wire.OutputNode == this._selected.Node).ToArray();
				foreach (var wire in deletedWires)
				{
					this._wires.Remove(wire);
				}

				this._nodes.Remove(this._selected);
				this._selected = null;
				this.CircuitPanel.Refresh();
			}
		}

		private void CircuitPanel_Paint(object sender, PaintEventArgs e)
		{
			this.RepaintCircuit(e.Graphics);
		}

		private void OnCircuitUpdate()
		{
			this.Invoke(new Action(this.CircuitPanel.Refresh)); // Refresh the panel on the main thread
		}

		// Tools
		private void RepaintCircuit(Graphics g)
		{
			// High quality drawing
			g.CompositingQuality = CompositingQuality.HighQuality;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.PixelOffsetMode = PixelOffsetMode.HighQuality;
			g.SmoothingMode = SmoothingMode.HighQuality;

			var halfRadius = this._portRadius / 2;

			// First Draw the wires
			foreach (var wire in this._wires)
			{
				var outputNode = this._nodes.First(n => n.Node == wire.Wire.OutputNode);
				var inputNode = this._nodes.First(n => n.Node == wire.Wire.InputNode);
				var inputIndex = wire.Wire.InputNumber - 1;

				Point from = this.CalculateOutputPortLocation(outputNode);
				Point to = this.CalculateInputPortLocation(inputNode, inputIndex);

				wire.Endpoint1 = from;
				wire.Endpoint2 = to;

				g.DrawLine(this._outlinePen, from, to);
			}

			// Second Draw the nodes
			foreach (var node in this._nodes)
			{
				var center = CalculateCenter(node.Rect);

				bool isNodeSelected = this._selected == node;
				bool isAnyPortSelected = this._selectedPort != null;

				// Draw Input ports
				foreach (var inputPos in this.CalculateInputPortLocations(node))
				{
					// Pick a pen based on whether this port is selected or not
					Pen inputPortOutLinePen = (this._selected == node && this._selectedPort == inputPos.Key) ? this._selectedOutlinePen : this._outlinePen;

					g.DrawLine(this._outlinePen, inputPos.Value, center);

					var inputPortRect = new RectangleF(inputPos.Value.X - halfRadius, inputPos.Value.Y - halfRadius, this._portRadius, this._portRadius);
					var isPortOn = this._circuit.GetInputPortState(node.Node, inputPos.Key);
					var inputBrush = isPortOn ? this._onBrush : this._offBrush;
					g.FillEllipse(inputBrush, inputPortRect);
					g.DrawEllipse(inputPortOutLinePen, inputPortRect);
				}

				var isOutput = this._circuit.GetNodeState(node.Node);
				// Draw Output Port
				if (!node.InputOnly)
				{
					var outputPos = this.CalculateOutputPortLocation(node);
					g.DrawLine(this._outlinePen, outputPos, center);
					var outputPortRect = new RectangleF(outputPos.X - halfRadius, outputPos.Y - halfRadius, this._portRadius, this._portRadius);
					Pen outputPortOutLinePen = (this._selected == node && this._selectedPort == 0) ? this._selectedOutlinePen : this._outlinePen;
					
					var outputBrush = isOutput ? this._onBrush : this._offBrush;
					g.FillEllipse(outputBrush, outputPortRect);
					g.DrawEllipse(outputPortOutLinePen, outputPortRect);
				}

				// Draw Icon
				var iconRect = this.CalculateIconRect(node.Rect, node.InputOnly);
				if (node.Node is AndNode) this.DrawAND(g, iconRect, node);
				else if (node.Node is OrNode) this.DrawOR(g, iconRect, node);
				else if (node.Node is XorNode) this.DrawXOR(g, iconRect, node);
				else if (node.Node is NotNode) this.DrawNOT(g, iconRect, node);
				else if (node.Node is SwitchNode) this.DrawSWITCH(g, iconRect, node, isOutput);
				else if (node.Node is BulbNode) this.DrawBulb(g, iconRect, node, isOutput);

				//this.DrawGateName(g, iconRect, node);
			}
		}

		private void AddNode(Node node)
		{
			var nodeEntry = new NodeEntry(node, new Point(150, 150), new Size(100,100));

			nodeEntry.Size =
				nodeEntry.InputOnly
				? new Size(100, 100 + this._portRadius * 2)
				: new Size(100 + this._portRadius * 2, 100);

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

		private WireEntry GetWireAt(Point location)
		{
			foreach (var wire in this._wires)
			{
				if (DistanceFrom(location, wire.Endpoint1, wire.Endpoint2) < WireSelectMaxDistance)
				{
					return wire;
				}
			}

			return null;
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
			var partSize =
				entry.InputOnly
				? (entry.Rect.Width - (this._portsMargin * 2)) / entry.Node.InputCount
				: (entry.Rect.Height - (this._portsMargin * 2)) / entry.Node.InputCount;

			var partHalf = partSize / 2;

			return
				entry.InputOnly
				? new Point(entry.Rect.X + (portIndex * partSize) + partHalf + this._portsMargin, entry.Rect.Bottom - this._portRadius) // Input port on bottom
				: new Point(entry.Rect.X + this._portRadius, entry.Rect.Y + (portIndex * partSize) + partHalf + this._portsMargin); // Input port on left
		}

		private Point CalculateOutputPortLocation(NodeEntry entry)
		{
			var heightHalf = entry.Size.Height / 2;

			return new Point(entry.Location.X + entry.Size.Width - this._portRadius, entry.Location.Y + heightHalf);
		}

		private Rectangle CalculateInteractiveZone(Rectangle backgroundRect)
		{
			var width = (int)(backgroundRect.Width * this._interactiveZoneRatio);
			var height = (int)(backgroundRect.Height * this._interactiveZoneRatio);

			var c = this.CalculateCenter(backgroundRect);
			return new Rectangle(c.X - (width / 2), c.Y - (height / 2), width, height);
		}

		private Rectangle CalculateIconRect(Rectangle nodeRectangle, bool isInputOnBottom)
		{
			var marginOnInputSide = this._iconMargin + this._portRadius;
			var marginOnStandardSide = this._iconMargin;

			var bottomMargin = isInputOnBottom ? marginOnInputSide : marginOnStandardSide;
			var leftMargin = isInputOnBottom ? marginOnStandardSide : marginOnInputSide;

			return new Rectangle(nodeRectangle.X + leftMargin, nodeRectangle.Y + this._iconMargin, nodeRectangle.Width -  (leftMargin * 2), nodeRectangle.Height - (bottomMargin * 2));
		}

		private bool IsNodeInputPortClicked(NodeEntry entry, Point clickLocation, out int portNumber)
		{
			for (int i = 0; i < entry.Node.InputCount; i++)
			{
				var pos = this.CalculateInputPortLocation(this._interacted, i);

				if (DistanceBetween(pos, clickLocation) < this._portRadius + this._outlinePen.Width)
				{
					portNumber = i + 1;
					return true;
				}
			}

			portNumber = -1;
			return false;
		}

		private bool IsNodeOutputPortClicked(NodeEntry entry, Point clickLocation)
		{
			Point outputPos = this.CalculateOutputPortLocation(entry);
			return DistanceBetween(outputPos, clickLocation) < this._portRadius + this._outlinePen.Width;
		}

		private Point CalculateCenter(Rectangle rect)
		{
			return new Point(rect.X + (rect.Width / 2), rect.Y + (rect.Height / 2));
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

		private void DrawSWITCH(Graphics g, Rectangle backgroundRect, NodeEntry entry, bool isOn)
		{
			g.FillRectangle(this._iconBackgroundBrush, backgroundRect);
			g.DrawRectangle(this._outlinePen, backgroundRect);

			var interactiveRect = this.CalculateInteractiveZone(backgroundRect);
			var brush = isOn ? this._onBrush : this._offBrush;
			g.FillRectangle(brush, interactiveRect);
			g.DrawRectangle(this._outlinePen, interactiveRect);
		}

		private void DrawBulb(Graphics g, Rectangle backgroundRect, NodeEntry entry, bool isOn)
		{
			var brush = isOn ? this._onBrush : this._offBrush;
			g.FillEllipse(brush, backgroundRect);
			g.DrawEllipse(this._outlinePen, backgroundRect);
		}

		private void DrawGateName(Graphics g, Rectangle backgroundRect, NodeEntry entry)
		{
			if (entry.Node.GateName != null)
			{
				var stringSize = g.MeasureString(entry.Node.GateName, this._gateNameFont);
				var center = CalculateCenter(entry.Rect);

				g.DrawString(entry.Node.GateName, this._gateNameFont, Brushes.Black, new Point(center.X - (int)(stringSize.Width / 2), center.Y - (int)(stringSize.Height / 2)));
			}
		}

		private bool Intersects(Point reference, Point offset, Size size)
		{
			return (reference.X >= offset.X && reference.X <= offset.X + size.Width)
				&& (reference.Y >= offset.Y && reference.Y <= offset.Y + size.Height);
		}

		private Double DistanceBetween(Point p1, Point p2)
		{
			var a = p1.X - p2.X;
			var b = p1.Y - p2.Y;
			return Math.Sqrt((a * a) + (b * b));
		}

		private double DistanceFrom(Point p, Point l1, Point l2)
		{
			var distFromL1 = DistanceBetween(p, l1);
			var distFromL2 = DistanceBetween(p, l2);
			var lineLength = DistanceBetween(l1, l2);

			if (distFromL1 <= lineLength && distFromL2 <= lineLength) // The point is closer to both endpoint than the length of line => the point is somewhere between the two endpoint
			{
				var y = l2.Y - l1.Y;
				var x = l2.X - l1.X;

				var top = Math.Abs((y * p.X) - (x * p.Y) + (l2.X * l1.Y) - (l2.Y * l1.X));
				var bottom = Math.Sqrt((y * y) + (x * x));

				return top / bottom;
			}
			else // Point is outside of the viccinity of the line
			{
				return double.MaxValue;
			}
		}

		private enum InteractionType
		{
			None,
			Pressed,
			Dragging,
		}
	}

	class NodeEntry
	{
		private Node _node;
		private Rectangle _rect;
		private bool _isInteractive;
		private bool _inputOnly;

		public Node Node
		{
			get => this._node;
			set
			{
				this._node = value;
				this._isInteractive = value is InteractiveNode;
			}
		}

		public Rectangle Rect
		{
			get => this._rect;
			set => this._rect = value;
		}

		public Point Location
		{
			get => this._rect.Location;
			set => this._rect.Location = value;
		}

		public Size Size
		{
			get => this._rect.Size;
			set => this._rect.Size = value;
		}

		public bool IsInteractive => this._isInteractive;

		public bool InputOnly => this._inputOnly;

		public NodeEntry(Node node, Point location, Size size)
		{
			this.Node = node;
			this._rect = new Rectangle(location, size);
			this._isInteractive = node is InteractiveNode;
			this._inputOnly = node is InputOnlyNode;
		}
	}

	class WireEntry
	{
		public LogicCircuit.Circuit.Wire Wire { get; }

		public Point Endpoint1 { get; set; }

		public Point Endpoint2 { get; set; }

		public WireEntry(LogicCircuit.Circuit.Wire wire)
		{
			this.Wire = wire;
		}
	}
}
