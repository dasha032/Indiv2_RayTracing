using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RayTracing
{
    using System.Windows.Forms;
    using FastBitmap;
    using Microsoft.VisualBasic;

    public delegate void RenderProgressHandler(double progress, TimeSpan elapsedTime);

    class RayTracing
    {
        public event RenderProgressHandler renderProgress;

        List<Shape> scene;
        const double fov = 80;
        Point cameraPosition = new Point(0, 0, 0);
        List<LightSource> lightSources;
        Room room;

        public RayTracing(Room room)
        {
            this.room = room;
            scene = new List<Shape>();
            lightSources = new List<LightSource>();
        }

        public void addShape(Shape shape)
        {
            scene.Add(shape);
        }

        public void addLightSource(LightSource lightSource)
        {
            lightSources.Add(lightSource);
        }

        public void clear()
        {
            scene.Clear();
            lightSources.Clear();
        }

        static Color changeColorIntensity(Color color, double intensity)
        {
            if (intensity < 0)
            {
                throw new Exception("Intensity must be >= 0!");
            }

            return Color.FromArgb((byte) Math.Clamp(Math.Round(color.R * intensity), 0, 255),
                (byte) Math.Clamp(Math.Round(color.G * intensity), 0, 255),
                (byte) Math.Clamp(Math.Round(color.B * intensity), 0, 255));
        }

        bool doesRayIntersectSomething(Vector direction, Point origin, out Tuple<Point, Vector> intersection)
        {
            intersection = null;
            Tuple<Point, Vector>  intersectionP;
            foreach (var shape in scene)
            {
                if (shape is Face)
                {
                    continue;
                }
                
                if (shape.getIntersection(direction, origin) != null)
                {
                    intersection = shape.getIntersection(direction, origin);
                    //intersection = new Tuple<Shape, Vector>(shape, intersectionP.Item2);
                    return true;
                }
            }

            return false;
        }
        Vector getLightReflectionRay(Vector shadowRay, Vector normale)
        {
            return (2 * (shadowRay ^ normale) * normale - shadowRay).normalize();
        }
        
        Vector getViewReflectionRay(Vector viewRay, Vector normale)
        {
            return (2 * ((-1 * viewRay) ^ normale) * normale - (-1 * viewRay)).normalize();
        }
        Vector getTransparencyRay(Vector viewRay, Vector normale, float n1, float n2)
        {
            // Нормализуем входящий луч и нормаль
            viewRay.normalize();
            normale.normalize();

            // Вычисляем отношение показателей преломления
            double eta = n1 / n2;

            // Вычисляем скалярное произведение (N • I)
            double cosThetaI = (normale ^ viewRay); // Угол между нормалью и направлением луча

            // Проверяем условие полного внутреннего отражения
            double sin2ThetaT = eta * eta * (1 - cosThetaI * cosThetaI); // sin^2(theta_t)
           /* if (sin2ThetaT > 1.0f)
            {
                // Полное внутреннее отражение, возвращаем пустой или ошибочный вектор
                //return new Vector(0,0,0);
                return getLightReflectionRay(viewRay, normale);
            }*/

            // Вычисляем cos(theta_t) с использованием формулы: cos(theta_t) = sqrt(1 - sin^2(theta_t))
            double cosThetaT = (float)Math.Sqrt(1.0f - sin2ThetaT);

            // Вычисляем направление преломленного луча:
            // T = eta * I + (eta * cosThetaI - cosThetaT) * N
            Vector refractedRay = eta * viewRay + (eta * cosThetaI - cosThetaT) * normale;

            // Возвращаем нормализованный преломленный луч
            refractedRay.normalize();
            return refractedRay;
        }
        Color mixColors(Color first, Color second, double secondToFirstRatio)
        {

            int red = (int)(first.R * (1 - secondToFirstRatio) + second.R * secondToFirstRatio);
            int green = (int)(first.G * (1 - secondToFirstRatio) + second.G * secondToFirstRatio);
            int blue = (int)(first.B * (1 - secondToFirstRatio) + second.B * secondToFirstRatio);


            return Color.FromArgb(red, green, blue);
        }
        double computeLightness(Shape shape, Tuple<Point, Vector> intersectionAndNormale, Vector viewRay)
        {
            double diffuseLightness = 0;
            double specularLightness = 0;
            double ambientLightness = 1;

            var intersectionPoint = intersectionAndNormale.Item1;
            var normale = intersectionAndNormale.Item2;
            foreach (var lightSource in lightSources)
            {
                // Создаем теневой луч от точки пересечения к источнику света
                var shadowRay = new Vector(intersectionPoint, lightSource.location, true);

                // Проверяем, находится ли точка в тени
                //bool isInShadow = false;
                Tuple<Point, Vector> shadowIntersection;
                if (doesRayIntersectSomething(shadowRay, intersectionAndNormale.Item1, out shadowIntersection))
                {
                    var intersectedShape = shadowIntersection.Item1; 
                    if (new Vector(intersectionAndNormale.Item1, shadowIntersection.Item1).Length() <
                        shadowRay.Length())
                        continue;
                }


                // Если объект в тени, пропускаем текущий источник света


                
                

                // Диффузное освещение
                diffuseLightness += lightSource.intensity *
                                    Math.Clamp(shadowRay ^ normale, 0.0, double.MaxValue);

                // Спекулярное освещение
                var reflectionRay = getLightReflectionRay(shadowRay, normale);
                specularLightness += lightSource.intensity *
                                        Math.Pow(Math.Clamp(reflectionRay ^ (-1 * viewRay), 0.0, double.MaxValue),
                                                shape.material.shininess);
                
            }

            // Итоговое освещение с учетом коэффициентов материала
            return ambientLightness * shape.material.kambient +
                   diffuseLightness * shape.material.kdiffuse +
                   specularLightness * shape.material.kspecular;
        }




        Color shootRay(Vector viewRay, Point origin, int depth = 0)
        {
            const int MAX_DEPTH = 10;
            if (depth > MAX_DEPTH)
                return Color.Gray; // Ограничение глубины трассировки

            double nearestPoint = double.MaxValue;
            Shape nearestShape = null;
            Tuple<Point, Vector> nearestIntersectionAndNormale = null;

            // Поиск ближайшего пересечения
            foreach (var shape in scene)
            {
                Tuple<Point, Vector> intersectionAndNormale;
                if ((intersectionAndNormale = shape.getIntersection(viewRay, origin)) != null &&
                    intersectionAndNormale.Item1.z < nearestPoint)
                {
                    nearestPoint = intersectionAndNormale.Item1.z;
                    nearestShape = shape;
                    nearestIntersectionAndNormale = intersectionAndNormale;
                }
                
            }

            // Если пересечений нет — возвращаем фон
            if (nearestShape == null || nearestIntersectionAndNormale == null)
                return Color.Black;

            // Освещение в точке пересечения
            var baseLightness = computeLightness(nearestShape, nearestIntersectionAndNormale, viewRay);
            Color resultColor = changeColorIntensity(nearestShape.color, baseLightness);

            // Обработка отражений
            if (nearestShape.material.reflectivity > 0)
            {
                Vector reflectedRay = getViewReflectionRay(viewRay, nearestIntersectionAndNormale.Item2);
                var reflectedColor = shootRay(reflectedRay, nearestIntersectionAndNormale.Item1, depth + 1);
                resultColor = mixColors(resultColor, reflectedColor, nearestShape.material.reflectivity);
            }

            // Обработка прозрачности
            if (nearestShape.material.transparency > 0)
            {
                Vector normale = nearestIntersectionAndNormale.Item2;
                Vector refractedRay = getTransparencyRay(viewRay, normale, 1.0f, (float)nearestShape.material.transparency);

                var transparentColor = shootRay(refractedRay, nearestIntersectionAndNormale.Item1, depth + 1);
                resultColor = mixColors(resultColor, transparentColor, nearestShape.material.transparency);
            }

            return resultColor;
        }



        public Bitmap compute(Size frameSize)
        {
            var start = DateTime.Now;
            onProgressIncremented(0.0, DateTime.Now - start);
            var bitmap = new Bitmap(frameSize.Width, frameSize.Height);
            int processedPixelCount = 0;
            double totalPixelCount = frameSize.Width * frameSize.Height;

            using (FastBitmap fbitmap = new FastBitmap(bitmap))
            {
                for (int x = 0; x < frameSize.Width; x++)
                {
                    for (int y = 0; y < frameSize.Height; y++)
                    {
                        Vector ray = new Vector(
                            (2 * (x + 0.5) / frameSize.Width - 1) * Math.Tan(Geometry.degreesToRadians(fov / 2)) *
                            frameSize.Width / frameSize.Height,
                            -(2 * (y + 0.5) / frameSize.Height - 1) * Math.Tan(Geometry.degreesToRadians(fov / 2)),
                            1, true);
                        var color = shootRay(ray, cameraPosition);
                        fbitmap.SetPixel(new System.Drawing.Point(x, y), color);
                        processedPixelCount++;
                        if (y % 10 == 0)
                        {
                            onProgressIncremented(processedPixelCount / totalPixelCount, DateTime.Now - start);
                        }
                    }
                }
            }

            return bitmap;
        }

        void onProgressIncremented(double progress, TimeSpan time)
        {
            renderProgress?.Invoke(progress, time);
        }
    }
}