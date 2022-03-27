using SlipeServer.Server.Elements;
using System.Collections.Generic;
using System.Timers;

namespace SlipeServer.Server.Behaviour
{
    public class VelocityBehaviour
    {
        private readonly float factor;

        private readonly HashSet<Element> velocityElements;
        private readonly Timer timer;

        public VelocityBehaviour(MtaServer server, float interval = 10)
        {
            this.factor = interval / 50.0f;

            this.velocityElements = new HashSet<Element>();
            this.timer = new Timer(interval)
            {
                AutoReset = true,
                Enabled = true
            };
            this.timer.Elapsed += OnTimerElapsed;

            server.ElementCreated += OnElementCreate;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            foreach (var element in this.velocityElements)
            {
                element.Position += (element.Velocity * this.factor);
            }
        }

        private void OnElementCreate(Element element)
        {
            if (element is not Ped && element is not Vehicle)
            {
                element.VelocityChanged += (sender, args) =>
                {
                    if (args.NewValue.Length() > 0)
                    {
                        if (!this.velocityElements.Contains(args.Source))
                            this.velocityElements.Add(args.Source);
                    } else
                    {
                        if (this.velocityElements.Contains(args.Source))
                            this.velocityElements.Remove(args.Source);
                    }
                };
            }
        }
    }
}
