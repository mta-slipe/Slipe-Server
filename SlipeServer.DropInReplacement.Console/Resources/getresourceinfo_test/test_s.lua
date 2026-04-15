local version = getResourceInfo(resource, "version")
assert(version == "2.5", "Expected version to be '2.5', got: " .. tostring(version))

local author = getResourceInfo(resource, "author")
assert(author == "TestAuthor", "Expected author to be 'TestAuthor', got: " .. tostring(author))

local missing = getResourceInfo(resource, "doesnotexist")
assert(missing == nil, "Expected nil for missing attribute, got: " .. tostring(missing))
