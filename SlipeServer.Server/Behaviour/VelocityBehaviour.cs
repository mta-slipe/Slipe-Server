using SlipeServer.Server.Elements;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

namespace SlipeServer.Server.Behaviour;

public class VelocityBehaviour
{
    private readonly float factor;

    private readonly ConcurrentDictionary<Element, Element> velocityElements;
    private readonly Timer timer;
    private readonly Stopwatch stopwatch;
    private readonly float interval;

    public VelocityBehaviour(MtaServer server, float interval = 50)
    {
        this.factor = 20f / 1000 * interval;

        this.velocityElements = new();
        this.timer = new Timer(interval)
        {
            AutoReset = true,
            Enabled = true
        };
        this.timer.Elapsed += OnTimerElapsed;
        this.stopwatch = new();
        this.stopwatch.Start();

        server.ElementCreated += OnElementCreate;
        this.interval = interval;
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var delta = this.stopwatch.ElapsedMilliseconds;
        this.stopwatch.Restart();
        foreach (var (_, element) in this.velocityElements)
        {
            element.Position += (element.Velocity * this.factor * (delta / this.interval));
        }
    }

    private void OnElementCreate(Element element)
    {
        if (element is not Ped && element is not Vehicle)
        {
            if (element.Velocity.Length() > 0)
                if (!this.velocityElements.ContainsKey(element))
                    this.velocityElements[element] = element;

            element.VelocityChanged += (sender, args) =>
            {
                if (args.NewValue.Length() > 0)
                {
                    if (!this.velocityElements.ContainsKey(args.Source))
                        this.velocityElements[args.Source] = args.Source;
                } else
                {
                    if (this.velocityElements.ContainsKey(args.Source))
                        this.velocityElements.Remove(args.Source, out var value);
                }
            };

            element.Destroyed += (source) =>
            {
                if (this.velocityElements.ContainsKey(source))
                    this.velocityElements.Remove(source, out var value);
            };
        }
    }
}
