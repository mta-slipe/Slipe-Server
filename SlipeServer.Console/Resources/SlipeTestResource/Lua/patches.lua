
function prePatches()
    
    bit = {
        bnot = bitNot,
        band = bitAnd,
        bor = bitOr,
        bxor = bitXor,
        lshift = bitLShift,
        rshift = bitRShift
    }

    function require() end

    setmetatable(_G, {
        __newindex = function() 
            duringPatches()
        end
    })
end

function duringPatches()
    
    isSystemIsPatched = false
    if (not isSytemIsPatched and System and System.is) then
        local oldIs = System.is;
        local function is(obj, T)
            return type(obj) == "userdata" and T == SlipeLua.MtaDefinitions.MtaElement or oldIs(obj, T)
        end
        System.is = is
        isSytemIsPatched = true
    end
end

function postPatches()
    
    setmetatable(_G, nil)
end

prePatches()
