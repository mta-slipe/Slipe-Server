---
title: Welcome to DocFX website!
documentType: index
---

<div class="hero">
  <div class="wrap">
    <div class="col-lg-offset-2 col-lg-4 col-sm-offset-1 col-sm-4 col-xs-12">
      <img class="hero-image" src="images/logo_large.png">
    </div>
    <div class="media-icons col-lg-4 col-sm-6 col-xs-12">
        <a href="https://github.com/mta-slipe/slipe-server" target="_blank"><i class="fab fa-github"></i></a>
        <a href="http://discord.gg/T4gkRFV" target="_blank"><i class="fab fa-discord"></i></a>
    </div>
    <div class="col-lg-4 col-sm-6 col-sm-12 col-xs-12">
      <h1 class="title main-title">Slipe Server</h1>
      <h3 class="title sub-title">Sharpen your MTA development experience</h3>
      <div class="buttons-unit">
          <a href="/articles/getting-started/getting-started.html" class="button b-1"><i class="glyphicon glyphicon-chevron-right"></i>Getting Started</a>
          <a href="https://www.nuget.org/packages/SlipeServer.Server/" class="button b-2 js-download-button"><i class="glyphicon glyphicon-download-alt"></i>NuGet Package</a>
      </div>
    </div>
  </div>
</div>

<div class="container">
  <div class="wrap row main-info">
      <div class="col-md-4 col-md-offset-0 col-xs-offset-1 col-xs-10">
        <h3>Native C# for MTA</h3>
        <p>
          With Slipe Server you're able to run a Native C# MTA server. Making use of the entire dotnet ecosytem, any NuGet package you want, industry-standard ORMs and logging frameworks, and far more.
        </p>
        <p>
          Slipe Server enables you to make use of the full power of C#. This includes but is not limited to: type safety, inheritance, multithreading and async/await.
        </p>
      </div>
      <div class="col-md-4 col-md-offset-0 col-xs-offset-1 col-xs-10">
        <h3>Built to be configurable</h3>
        <p>
          Slipe Server is built from the ground up to be configurable. You can modify anything, like:
          <ul>
            <li>Tweaking the processing of sync packets</li>
            <li>Completely overhauling (or even replacing) the processing of sync packets</li>
            <li>Implementing custom element classes for your gamemode logic</li>
            <li>Changing how elements are created to multiple clients</li>
          </ul>
        </p>
        <p>
          Build your projects in <a href="https://visualstudio.microsoft.com/" target="_blank">Visual Studio</a>, and let <a href="https://visualstudio.microsoft.com/services/intellicode/" target="_blank">IntelliSense and IntelliCode</a> increase your productivity even more!
        </p>
      </div>
      <div class="col-md-4 col-md-offset-0 col-xs-offset-1 col-xs-10">
        <h3>(First party) Extensions</h3>
        <p>
          Besides being configurable some very powerful (first party) extensions are available. These can provide better development experiences, useful new features, scripting support and more.
        </p>
        <p>
          Some examples include:
          <ul>
            <li>
              <a href="https://www.nuget.org/packages/SlipeServer.Physics/" target="_blank">Server Side physics and raycasting</a>
            </li>
            <li>
              <a href="https://www.nuget.org/packages/SlipeServer.LuaControllers/" target="_blank">library to write ASP.NET MVC-like controllers for your Lua event handling</a>
            </li>
          </ul>
        </p>
      </div>
  </div>
</div>

<section class="example-section">
  <div class="container">
    <div class="example-block-container">
        <div class="row">
          <div class="col-md-8">
            <pre>
            <code class="lang-csharp">
[<span class="hljs-meta">LuaController(<span class="hljs-string">""</span>)</span>]
<span class="hljs-keyword">public</span> <span class="hljs-keyword">class</span> <span class="hljs-title">VehicleController</span> : <span class="hljs-title">BaseLuaController</span>
{
    <span class="hljs-keyword">private</span> <span class="hljs-keyword">readonly</span> MtaServer server;
    <span class="hljs-keyword">private</span> <span class="hljs-keyword">readonly</span> VehicleUpgradeService vehicleUpgradeService;
    <span></span>
    <i id="e1" onmouseenter="document.getElementById('i1').className = 'forceHover';" onmouseleave="document.getElementById('i1').className = '';"><span class="hljs-function"><span class="hljs-keyword">public</span> <span class="hljs-title">VehicleController</span>(<span class="hljs-params">MtaServer server, VehicleUpgradeService vehicleUpgradeService</span>)</span></i>
    {
        <span class="hljs-keyword">this</span>.server = server;
        <span class="hljs-keyword">this</span>.vehicleUpgradeService = vehicleUpgradeService;
    }
    <span></span>
    <i id="e2" onmouseenter="document.getElementById('i2').className = 'forceHover';" onmouseleave="document.getElementById('i2').className = '';">[<span class="hljs-meta">LuaEvent(<span class="hljs-string">"giveMeVehicles"</span>)</span>]</i>
    <span class="hljs-function"><span class="hljs-keyword">public</span> <span class="hljs-keyword">void</span> <span class="hljs-title">CreateVehicleForPlayer</span>(<span class="hljs-params"><span class="hljs-built_in">int</span> model</span>)</span>
    {
        <i id="e3" onmouseenter="document.getElementById('i3').className = 'forceHover';" onmouseleave="document.getElementById('i3').className = '';"><span class="hljs-keyword">var</span> position = <span class="hljs-keyword">this</span>.Context.Player.Position + <span class="hljs-keyword">new</span> Vector3(<span class="hljs-number">0</span>, <span class="hljs-number">0</span>, <span class="hljs-number">2</span>);</i>
        <i id="e4" onmouseenter="document.getElementById('i4').className = 'forceHover';" onmouseleave="document.getElementById('i4').className = '';"><span class="hljs-keyword">var</span> vehicle = <span class="hljs-keyword">new</span> FreeroamVehicle((<span class="hljs-built_in">ushort</span>)model, position)
        {
            Rotation = <span class="hljs-keyword">this</span>.Context.Player.Rotation,
            Interior = <span class="hljs-keyword">this</span>.Context.Player.Interior,
            Dimension = <span class="hljs-keyword">this</span>.Context.Player.Dimension,
        };</i>
    <span></span>
        vehicle.AssociateWith(<span class="hljs-keyword">this</span>.server);
        vehicle.Driver = <span class="hljs-keyword">this</span>.Context.Player;
        <span class="hljs-keyword">this</span>.Context.Player.Vehicles.Add(vehicle);
    }
}</code>
            </pre>
          </div>
          <div class="col-md-4">
            <h3>Take advantage of C#!</h3>
            <p class="side-info">Slipe Server is a complete C# implementation of the MTA server platform. The benefits of using C# in combination with Visual Studio are innumerable. Everything that works in C# works in Slipe. Take a look:</p>
            <ul class="side-info">
              <li id="i1" onmouseenter="document.getElementById('e1').className = 'forceHover';" onmouseleave="document.getElementById('e1').className = '';">Use Dependency Injection</li>
              <li id="i2" onmouseenter="document.getElementById('e2').className = 'forceHover';" onmouseleave="document.getElementById('e2').className = '';">Handle Lua Events simply by defining methods</li>
              <li id="i3" onmouseenter="document.getElementById('e3').className = 'forceHover';" onmouseleave="document.getElementById('e3').className = '';">Use System.Numerics and other powerful .NET libraries</li>
              <li id="i4" onmouseenter="document.getElementById('e4').className = 'forceHover';" onmouseleave="document.getElementById('e4').className = '';">Typesafe constructors, methods and properties</li>
            </ul>
          </div>
        </div>
    </div>
  </div>
</section>

<i id="e1" onmouseenter="document.getElementById('i1').className = 'forceHover';" onmouseleave="document.getElementById('e1').className = '';">
</i>

<section class="section-contributions">
  <div class="container">
    <div class="col-md-8">
      <h3>Community</h3>
      <p>
        We are incredibly grateful for everyone who puts energy in making Slipe Server a bit better every day. This applies to our <a href="https://github.com/mta-slipe/Slipe-Core/graphs/contributors">GitHub contributors</a> but also everyone who is involved with the project on <a href="http://discord.gg/T4gkRFV" target="_blank">Discord</a>. Join the Slipe community and sharpen your MTA development experience!
      </p>
      <div class="col-sm-6 col-xs-12">
        <h4>Project Contributors</h4>
        <ul class="js-contributor-list">
        </ul>
      </div>
      <div class="col-sm-6 col-xs-12">
          <a href="https://github.com/mta-slipe" target="_blank"><i class="fab fa-github"></i></a>
          <a href="http://discord.gg/T4gkRFV" target="_blank"><i class="fab fa-discord"></i></a>
      </div>
    </div>
    <div class="col-md-4">
    </div>
  </div>
</section>
<style type="text/css">
  footer{
    position: relative;
  }
</style>