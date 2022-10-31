-- Generated by CSharp.lua Compiler
local System = System
local SlipeLuaMtaDefinitions = SlipeLua.MtaDefinitions
local SlipeLuaSharedElements = SlipeLua.Shared.Elements
local SlipeLuaSharedIO = SlipeLua.Shared.IO
local ArrayString = System.Array(System.String)
System.namespace("SlipeLua.Client.Browsers", function (namespace)
  --/ <summary>
  --/ Class that wraps MTA browsers
  --/ </summary>
  namespace.class("Browser", function (namespace)
    local getSettings, getCanNavigateBack, getCanNavigateForward, getTitle, getUrl, getIsLoading, getIsFocused, setVolume, 
    setRenderingPaused, setDevTools, ReloadPage, LoadUrl, Focus, GetProperty, InjectMouseDown, InjectMouseUp, 
    InjectMouseMove, InjectMouseWheel, Resize, ExecuteJavascript, ExecuteJavascript1, IsDomainBlocked, RequestDomains, RequestDomain, 
    HandleDomainRequest, addOnDomainRequestAccepted, removeOnDomainRequestAccepted, addOnDomainRequestDenied, removeOnDomainRequestDenied, addOnCreated, removeOnCreated, addOnCursorChange, 
    removeOnCursorChange, addOnDocumentReady, removeOnDocumentReady, addOnInputFocusChange, removeOnInputFocusChange, addOnLoadFail, removeOnLoadFail, addOnLoadStart, 
    removeOnLoadStart, addOnNavigate, removeOnNavigate, addOnPopup, removeOnPopup, addOnResourceBlocked, removeOnResourceBlocked, addOnTooltip, 
    removeOnTooltip, addOnWhiteListChange, removeOnWhiteListChange, class, __ctor1__, __ctor2__
    __ctor1__ = function (this, element)
      SlipeLuaSharedElements.Element.__ctor__[2](this, element)
    end
    __ctor2__ = function (this, width, height, isLocal, transparent)
      __ctor1__(this, SlipeLuaMtaDefinitions.MtaClient.CreateBrowser(width, height, isLocal, transparent))
    end
    getSettings = function ()
      return SlipeLuaMtaDefinitions.MtaClient.GetBrowserSettings()
    end
    getCanNavigateBack = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.CanBrowserNavigateBack(this.element)
    end
    getCanNavigateForward = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.CanBrowserNavigateForward(this.element)
    end
    getTitle = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GetBrowserTitle(this.element)
    end
    getUrl = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.GetBrowserURL(this.element)
    end
    getIsLoading = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.IsBrowserLoading(this.element)
    end
    getIsFocused = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.IsBrowserFocused(this.element)
    end
    setVolume = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetBrowserVolume(this.element, value)
    end
    setRenderingPaused = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.SetBrowserRenderingPaused(this.element, value)
    end
    setDevTools = function (this, value)
      SlipeLuaMtaDefinitions.MtaClient.ToggleBrowserDevTools(this.element, value)
    end
    ReloadPage = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.ReloadBrowserPage(this.element)
    end
    LoadUrl = function (this, url, postData, urlEncoded)
      return SlipeLuaMtaDefinitions.MtaClient.LoadBrowserURL(this.element, url, postData, urlEncoded)
    end
    Focus = function (this)
      return SlipeLuaMtaDefinitions.MtaClient.FocusBrowser(this.element)
    end
    GetProperty = function (this, key)
      return SlipeLuaMtaDefinitions.MtaClient.GetBrowserProperty(this.element, key)
    end
    InjectMouseDown = function (this, mouseButton)
      return SlipeLuaMtaDefinitions.MtaClient.InjectBrowserMouseDown(this.element, mouseButton:EnumToString(SlipeLuaSharedIO.MouseButton):ToLower())
    end
    InjectMouseUp = function (this, mouseButton)
      return SlipeLuaMtaDefinitions.MtaClient.InjectBrowserMouseUp(this.element, mouseButton:EnumToString(SlipeLuaSharedIO.MouseButton):ToLower())
    end
    InjectMouseMove = function (this, position)
      return SlipeLuaMtaDefinitions.MtaClient.InjectBrowserMouseMove(this.element, System.ToInt32(position.X), System.ToInt32(position.Y))
    end
    InjectMouseWheel = function (this, vertical, horizontal)
      return SlipeLuaMtaDefinitions.MtaClient.InjectBrowserMouseWheel(this.element, vertical, horizontal)
    end
    Resize = function (this, dimensions)
      return SlipeLuaMtaDefinitions.MtaClient.ResizeBrowser(this.element, dimensions.X, dimensions.Y)
    end
    ExecuteJavascript = function (this, javascript)
      return SlipeLuaMtaDefinitions.MtaClient.ExecuteBrowserJavascript(this.element, javascript)
    end
    ExecuteJavascript1 = function (this, function_, arguments)
      local javascriptString = function_ .. "("

      for _, argument in System.each(arguments) do
        javascriptString = javascriptString .. (argument:ToString() .. ", ")
      end
      javascriptString = javascriptString:Substring(0, #javascriptString - 2)

      javascriptString = javascriptString .. ")"
      return ExecuteJavascript(this, javascriptString)
    end
    IsDomainBlocked = function (domain, isURL)
      return SlipeLuaMtaDefinitions.MtaClient.IsBrowserDomainBlocked(domain, isURL)
    end
    RequestDomains = function (domains, isURL)
      return SlipeLuaMtaDefinitions.MtaClient.RequestBrowserDomains(domains, isURL, HandleDomainRequest)
    end
    RequestDomain = function (domain, isURL)
      return RequestDomains(ArrayString(1, { domain }), isURL)
    end
    HandleDomainRequest = function (wasAccepted, domains)
      for i = 0, #domains - 1 do
        local domain = domains:get(i)
        if wasAccepted then
          local default = class.OnDomainRequestAccepted
          if default ~= nil then
            default(domain)
          end
        else
          local default = class.OnDomainRequestDenied
          if default ~= nil then
            default(domain)
          end
        end
      end
    end
    addOnDomainRequestAccepted = function (value)
      class.OnDomainRequestAccepted = class.OnDomainRequestAccepted + value
    end
    removeOnDomainRequestAccepted = function (value)
      class.OnDomainRequestAccepted = class.OnDomainRequestAccepted - value
    end
    addOnDomainRequestDenied = function (value)
      class.OnDomainRequestDenied = class.OnDomainRequestDenied + value
    end
    removeOnDomainRequestDenied = function (value)
      class.OnDomainRequestDenied = class.OnDomainRequestDenied - value
    end
    addOnCreated, removeOnCreated = System.event("OnCreated")
    addOnCursorChange, removeOnCursorChange = System.event("OnCursorChange")
    addOnDocumentReady, removeOnDocumentReady = System.event("OnDocumentReady")
    addOnInputFocusChange, removeOnInputFocusChange = System.event("OnInputFocusChange")
    addOnLoadFail, removeOnLoadFail = System.event("OnLoadFail")
    addOnLoadStart, removeOnLoadStart = System.event("OnLoadStart")
    addOnNavigate, removeOnNavigate = System.event("OnNavigate")
    addOnPopup, removeOnPopup = System.event("OnPopup")
    addOnResourceBlocked, removeOnResourceBlocked = System.event("OnResourceBlocked")
    addOnTooltip, removeOnTooltip = System.event("OnTooltip")
    addOnWhiteListChange = function (value)
      class.OnWhiteListChange = class.OnWhiteListChange + value
    end
    removeOnWhiteListChange = function (value)
      class.OnWhiteListChange = class.OnWhiteListChange - value
    end
    class = {
      base = function (out)
        return {
          out.SlipeLua.Shared.Elements.Element
        }
      end,
      getSettings = getSettings,
      getCanNavigateBack = getCanNavigateBack,
      getCanNavigateForward = getCanNavigateForward,
      getTitle = getTitle,
      getUrl = getUrl,
      getIsLoading = getIsLoading,
      getIsFocused = getIsFocused,
      setVolume = setVolume,
      setRenderingPaused = setRenderingPaused,
      setDevTools = setDevTools,
      ReloadPage = ReloadPage,
      LoadUrl = LoadUrl,
      Focus = Focus,
      GetProperty = GetProperty,
      InjectMouseDown = InjectMouseDown,
      InjectMouseUp = InjectMouseUp,
      InjectMouseMove = InjectMouseMove,
      InjectMouseWheel = InjectMouseWheel,
      Resize = Resize,
      ExecuteJavascript = ExecuteJavascript,
      ExecuteJavascript1 = ExecuteJavascript1,
      IsDomainBlocked = IsDomainBlocked,
      RequestDomains = RequestDomains,
      RequestDomain = RequestDomain,
      HandleDomainRequest = HandleDomainRequest,
      addOnDomainRequestAccepted = addOnDomainRequestAccepted,
      removeOnDomainRequestAccepted = removeOnDomainRequestAccepted,
      addOnDomainRequestDenied = addOnDomainRequestDenied,
      removeOnDomainRequestDenied = removeOnDomainRequestDenied,
      addOnCreated = addOnCreated,
      removeOnCreated = removeOnCreated,
      addOnCursorChange = addOnCursorChange,
      removeOnCursorChange = removeOnCursorChange,
      addOnDocumentReady = addOnDocumentReady,
      removeOnDocumentReady = removeOnDocumentReady,
      addOnInputFocusChange = addOnInputFocusChange,
      removeOnInputFocusChange = removeOnInputFocusChange,
      addOnLoadFail = addOnLoadFail,
      removeOnLoadFail = removeOnLoadFail,
      addOnLoadStart = addOnLoadStart,
      removeOnLoadStart = removeOnLoadStart,
      addOnNavigate = addOnNavigate,
      removeOnNavigate = removeOnNavigate,
      addOnPopup = addOnPopup,
      removeOnPopup = removeOnPopup,
      addOnResourceBlocked = addOnResourceBlocked,
      removeOnResourceBlocked = removeOnResourceBlocked,
      addOnTooltip = addOnTooltip,
      removeOnTooltip = removeOnTooltip,
      addOnWhiteListChange = addOnWhiteListChange,
      removeOnWhiteListChange = removeOnWhiteListChange,
      __ctor__ = {
        __ctor1__,
        __ctor2__
      },
      __metadata__ = function (out)
        return {
          properties = {
            { "CanNavigateBack", 0x206, System.Boolean, getCanNavigateBack },
            { "CanNavigateForward", 0x206, System.Boolean, getCanNavigateForward },
            { "DevTools", 0x306, System.Boolean, setDevTools },
            { "IsFocused", 0x206, System.Boolean, getIsFocused },
            { "IsLoading", 0x206, System.Boolean, getIsLoading },
            { "RenderingPaused", 0x306, System.Boolean, setRenderingPaused },
            { "Settings", 0x20E, System.Object, getSettings },
            { "Title", 0x206, System.String, getTitle },
            { "Url", 0x206, System.String, getUrl },
            { "Volume", 0x306, System.Single, setVolume }
          },
          methods = {
            { ".ctor", 0x106, __ctor1__, out.SlipeLua.MtaDefinitions.MtaElement },
            { ".ctor", 0x406, __ctor2__, System.Int32, System.Int32, System.Boolean, System.Boolean },
            { "ExecuteJavascript", 0x286, ExecuteJavascript1, System.String, System.IEnumerable_1(out.SlipeLua.Client.Browsers.JavascriptVariable), System.Boolean },
            { "ExecuteJavascript", 0x186, ExecuteJavascript, System.String, System.Boolean },
            { "Focus", 0x86, Focus, System.Boolean },
            { "GetProperty", 0x186, GetProperty, System.String, System.Object },
            { "HandleDomainRequest", 0x20E, HandleDomainRequest, System.Boolean, System.Array(System.String) },
            { "InjectMouseDown", 0x186, InjectMouseDown, System.Int32, System.Boolean },
            { "InjectMouseMove", 0x186, InjectMouseMove, System.Numerics.Vector2, System.Boolean },
            { "InjectMouseUp", 0x186, InjectMouseUp, System.Int32, System.Boolean },
            { "InjectMouseWheel", 0x286, InjectMouseWheel, System.Int32, System.Int32, System.Boolean },
            { "IsDomainBlocked", 0x28E, IsDomainBlocked, System.String, System.Boolean, System.Boolean },
            { "LoadUrl", 0x386, LoadUrl, System.String, System.String, System.Boolean, System.Boolean },
            { "ReloadPage", 0x86, ReloadPage, System.Boolean },
            { "RequestDomain", 0x28E, RequestDomain, System.String, System.Boolean, System.Boolean },
            { "RequestDomains", 0x28E, RequestDomains, System.Array(System.String), System.Boolean, System.Boolean },
            { "Resize", 0x186, Resize, System.Numerics.Vector2, System.Boolean }
          },
          class = { 0x6, System.new(out.SlipeLua.Shared.Elements.DefaultElementClassAttribute, 2, 31 --[[ElementType.Browser]]) }
        }
      end
    }
    return class
  end)
end)