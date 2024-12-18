using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RayTracing
{
    public enum SelectedItem
    {
        BackWall,
        RightWall,
        LeftWall,
        Floor,
        Ceiling,
        Cube,
        SphereOnCube,
        SphereOnGround
    }

    public partial class Form1 : Form
    {
        RayTracing rayTracing;

        // ========================== SCHEME DRAWING ==========================
        Rectangle backWallScheme;
        Rectangle cubeScheme;
        Rectangle sphereOnCubeScheme;
        Rectangle sphereOnGroundScheme;
        Rectangle lampScheme;

        Pen standardPen = new Pen(Color.Black, 2);
        Pen highlightPen = new Pen(Color.GreenYellow, 4);

        Graphics g;
        // ====================================================================

        SelectedItem currentItemType;
        Shape currentItem;

        Cube cube;
        Sphere sphereOnCube;
        Sphere sphereOnGround;
        Face leftWall;
        Face rightWall;
        Face backWall;
        Face cameraWall;
        Face ceiling;
        Face floor;
        Cube lamp;

        LightSource additionalLight = new LightSource(new Point(-7, 7, 0), 0.8);

        public Form1()
        {
            InitializeComponent();
            canvas.Image = new Bitmap(canvas.Width, canvas.Height);
            g = Graphics.FromImage(canvas.Image);

            var center = new Point(0, 0, 9);
            double roomSide = 20;

            initShapes(center, roomSide);
            //initScheme();

            rayTracing = new RayTracing(new Room(center, roomSide, leftWall, rightWall, backWall, floor, ceiling, cameraWall));
            //rayTracing.renderProgress += updateProgress;


            //labelTime.Visible = true;
            //progressBar.Visible = true;

            rayTracing.clear();

            rayTracing.addShape(sphereOnCube);

            rayTracing.addShape(cube);
            rayTracing.addShape(sphereOnGround);

            rayTracing.addShape(cameraWall);
            rayTracing.addShape(backWall);
            rayTracing.addShape(ceiling);
            rayTracing.addShape(floor);
            rayTracing.addShape(rightWall);
            rayTracing.addShape(leftWall);
            rayTracing.addLightSource(new LightSource(new Point(0, 7, 10), 1));
            //rayTracing.addLightSource(new LightSource(new Point(0, 7, 5), 1));


            runAsyncComputation();
          
        }

        void initShapes(Point center, double roomSide)
        {
            sphereOnGround = new Sphere(new Point(5, -5, 17), 6, Color.CornflowerBlue,
                new Material(10, 0.1, 0.85, 0.05, 0, 1));
            cube = new Cube(new Point(-5, -7, 16), 6, Color.Aquamarine, new Material(40, 0.25, 0.7, 0.05, 0, 0));
            sphereOnCube = new Sphere(new Point(-5, -2, 15), 2, Color.DarkSalmon,
                new Material(40, 0.25, 0.7, 0.05, 0, 1));

            leftWall = new Face(new Point(center.x - roomSide / 2, center.y, center.z), new Vector(1, 0, 0),
                new Vector(0, 1, 0), roomSide, roomSide, Color.Red, new Material(0, 0, 0.9, 0.1, 0, 0));
            rightWall = new Face(new Point(center.x + roomSide / 2, center.y, center.z), new Vector(-1, 0, 0),
                new Vector(0, 1, 0), roomSide, roomSide, Color.DarkBlue, new Material(0, 0, 0.9, 0.1, 0, 0));
            backWall = new Face(new Point(center.x, center.y, center.z + roomSide / 2), new Vector(0, 0, -1),
                new Vector(0, 1, 0), roomSide, roomSide, Color.LightGray, new Material(0, 0, 0.9, 0.1, 0, 0));
            cameraWall = new Face(new Point(center.x, center.y, center.z - roomSide / 2), new Vector(0, 0, 1), new Vector(0, 1, 0), roomSide, roomSide, Color.LightGray, new Material(0, 0, 0.9, 0.1, 0, 0));
            ceiling = new Face(new Point(center.x, center.y + roomSide / 2, center.z), new Vector(0, -1, 0), new Vector(0, 0, 1), roomSide, roomSide, Color.LightGray, new Material(0, 0, 0.9, 0.1, 0, 0));
            floor = new Face(new Point(center.x, center.y - roomSide / 2, center.z), new Vector(0, 1, 0), new Vector(0, 0, 1), roomSide, roomSide, Color.LightGray, new Material(0, 0, 0.9, 0.1, 0, 0));

        }

       

        async void runAsyncComputation()
        {
            var bitmap = rayTracing.compute(new Size(canvas.Width, canvas.Height));
            canvas.Image = bitmap;
        }


        void redrawScheme()
        {
            g.Clear(Color.Gray);
            Pen backwallPen = standardPen,
                rightwallPen = standardPen,
                leftwallPen = standardPen,
                floorPen = standardPen,
                ceilingPen = standardPen,
                sphereOnCubePen = standardPen,
                sphereOnGroundPen = standardPen,
                cubePen = standardPen;
            switch (currentItemType)
            {
                case SelectedItem.BackWall:
                    backwallPen = highlightPen;
                    break;
                case SelectedItem.Cube:
                    cubePen = highlightPen;
                    break;
                case SelectedItem.LeftWall:
                    leftwallPen = highlightPen;
                    break;
                case SelectedItem.RightWall:
                    rightwallPen = highlightPen;
                    break;
                case SelectedItem.SphereOnCube:
                    sphereOnCubePen = highlightPen;
                    break;
                case SelectedItem.SphereOnGround:
                    sphereOnGroundPen = highlightPen;
                    break;
                case SelectedItem.Floor:
                    floorPen = highlightPen;
                    break;
                case SelectedItem.Ceiling:
                    ceilingPen = highlightPen;
                    break;
            }
            // ========================== COLORS ==========================
            g.FillRectangle(new SolidBrush(backWall.color), backWallScheme);

            g.FillPolygon(new SolidBrush(ceiling.color),
                new PointF[]
                {
                    new System.Drawing.Point(backWallScheme.Location.X, backWallScheme.Location.Y),
                    new System.Drawing.Point(0, 0),
                    new System.Drawing.Point(canvas.Width, 0),
                    new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width, backWallScheme.Location.Y),
                });
            g.FillPolygon(new SolidBrush(leftWall.color),
                new PointF[]
                {
                    new System.Drawing.Point(0, 0), backWallScheme.Location,
                    new System.Drawing.Point(backWallScheme.Location.X,
                        backWallScheme.Location.Y + backWallScheme.Height),
                    new System.Drawing.Point(0, canvas.Height)
                });
            g.FillPolygon(new SolidBrush(rightWall.color),
                new PointF[]
                {
                    new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width,
                        backWallScheme.Location.Y),
                    new System.Drawing.Point(canvas.Width, 0), new System.Drawing.Point(canvas.Width, canvas.Height),
                    new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width,
                        backWallScheme.Location.Y + backWallScheme.Height)
                });
            g.FillPolygon(new SolidBrush(floor.color),
                new PointF[]
                {
                    new System.Drawing.Point(backWallScheme.Left, backWallScheme.Bottom),
                    new System.Drawing.Point(0, canvas.Height),
                    new System.Drawing.Point(canvas.Width, canvas.Height),
                    new System.Drawing.Point(backWallScheme.Right, backWallScheme.Bottom),
                });

            // ========================== BACK WALL ==========================
            g.DrawLine(backwallPen, backWallScheme.Location,
                new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width, backWallScheme.Location.Y));
            g.DrawLine(backwallPen, backWallScheme.Location,
                new System.Drawing.Point(backWallScheme.Location.X, backWallScheme.Location.Y + backWallScheme.Height));
            g.DrawLine(backwallPen,
                new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width, backWallScheme.Location.Y),
                new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width,
                    backWallScheme.Location.Y + backWallScheme.Height));
            g.DrawLine(backwallPen,
                new System.Drawing.Point(backWallScheme.Location.X, backWallScheme.Location.Y + backWallScheme.Height),
                new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width,
                    backWallScheme.Location.Y + backWallScheme.Height));

            // ========================== LEFT WALL ==========================
            g.DrawLine(leftwallPen, backWallScheme.Location, new System.Drawing.Point(0, 0));
            g.DrawLine(leftwallPen,
                new System.Drawing.Point(backWallScheme.Location.X, backWallScheme.Location.Y + backWallScheme.Height),
                new System.Drawing.Point(0, canvas.Height));
            if (currentItemType == SelectedItem.LeftWall)
            {
                g.DrawLine(leftwallPen, backWallScheme.Location,
                    new System.Drawing.Point(backWallScheme.Location.X,
                        backWallScheme.Location.Y + backWallScheme.Height));
            }

            // ========================== RIGHT WALL ==========================
            g.DrawLine(rightwallPen,
                new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width, backWallScheme.Location.Y),
                new System.Drawing.Point(canvas.Width, 0));
            g.DrawLine(rightwallPen,
                new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width,
                    backWallScheme.Location.Y + backWallScheme.Height),
                new System.Drawing.Point(canvas.Width, canvas.Height));
            if (currentItemType == SelectedItem.RightWall)
            {
                g.DrawLine(rightwallPen,
                    new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width,
                        backWallScheme.Location.Y),
                    new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width,
                        backWallScheme.Location.Y + backWallScheme.Height));
            }

            // ========================== CEILING ==========================
            if (currentItemType != SelectedItem.RightWall)
            {
                g.DrawLine(ceilingPen,
                new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width, backWallScheme.Location.Y),
                new System.Drawing.Point(canvas.Width, 0));
            }
            if (currentItemType != SelectedItem.LeftWall)
            {
                g.DrawLine(ceilingPen,
                new System.Drawing.Point(backWallScheme.Location.X,
                    backWallScheme.Location.Y),
                new System.Drawing.Point(0, 0));
            }
            if (currentItemType == SelectedItem.Ceiling)
            {
                g.DrawLine(ceilingPen,
                    new System.Drawing.Point(backWallScheme.Location.X, backWallScheme.Location.Y),
                    new System.Drawing.Point(backWallScheme.Location.X + backWallScheme.Width, backWallScheme.Location.Y));
            }

            // ========================== FLOOR ==========================
            if (currentItemType != SelectedItem.RightWall)
            {
                g.DrawLine(floorPen,
                new System.Drawing.Point(backWallScheme.Right, backWallScheme.Bottom),
                new System.Drawing.Point(canvas.Width, canvas.Height));
            }
            if (currentItemType != SelectedItem.LeftWall)
            {
                g.DrawLine(floorPen,
                new System.Drawing.Point(backWallScheme.Left, backWallScheme.Bottom),
                new System.Drawing.Point(0, canvas.Height));
            }
            if (currentItemType == SelectedItem.Floor)
            {
                g.DrawLine(floorPen,
                    new System.Drawing.Point(backWallScheme.Left, backWallScheme.Bottom),
                    new System.Drawing.Point(backWallScheme.Right, backWallScheme.Bottom));
            }

            // ========================== OBJECTS ==========================
            g.FillRectangle(new SolidBrush(cube.color), cubeScheme);
            g.DrawRectangle(cubePen, cubeScheme);

            g.FillEllipse(new SolidBrush(sphereOnCube.color), sphereOnCubeScheme);
            g.DrawEllipse(sphereOnCubePen, sphereOnCubeScheme);

            g.FillEllipse(new SolidBrush(sphereOnGround.color), sphereOnGroundScheme);
            g.DrawEllipse(sphereOnGroundPen, sphereOnGroundScheme);

            g.FillEllipse(new SolidBrush(Color.Gold), new Rectangle(canvas.Width / 2 - 11, 20, 25, 7));

            canvas.Invalidate();
        }

        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (cubeScheme.Contains(e.Location))
            {
                currentItemType = SelectedItem.Cube;
                currentItem = cube;
            }
            else if (sphereOnCubeScheme.Contains(e.Location))
            {
                currentItemType = SelectedItem.SphereOnCube;
                currentItem = sphereOnCube;
            }
            else if (sphereOnGroundScheme.Contains(e.Location))
            {
                currentItemType = SelectedItem.SphereOnGround;
                currentItem = sphereOnGround;
            }
            else if (backWallScheme.Contains(e.Location))
            {
                currentItemType = SelectedItem.BackWall;
                currentItem = backWall;
            }
            else if (e.Location.X < backWallScheme.Location.X)
            {
                currentItemType = SelectedItem.LeftWall;
                currentItem = leftWall;
            }
            else if (e.Location.X > backWallScheme.Location.X + backWallScheme.Width)
            {
                currentItemType = SelectedItem.RightWall;
                currentItem = rightWall;
            }
            else if (e.Location.Y < backWallScheme.Location.Y)
            {
                currentItemType = SelectedItem.Ceiling;
                currentItem = ceiling;
            }
            else if (e.Location.Y > backWallScheme.Location.Y + backWallScheme.Height)
            {
                currentItemType = SelectedItem.Floor;
                currentItem = floor;
            }


            redrawScheme();
        }

      

        private void buttonChangeColor_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = currentItem.color;
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                currentItem.color = colorDialog1.Color;
                redrawScheme();
            }
        }

        
        private void canvas_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void labelTime_Click(object sender, EventArgs e)
        {

        }
    }
}