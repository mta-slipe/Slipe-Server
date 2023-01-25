exports.preTransform = function (model) {
  return model;
};

exports.postTransform = function (model) {
  handleItem(model, model._gitContribute, model._gitUrlPattern);
  if (model.children) {
    model.children.forEach(function (item) {
      handleItem(item, model._gitContribute, model._gitUrlPattern);
    });
  }

  return model;
};

function handleItem(vm, gitContribute, gitUrlPattern) {
  // set to empty string incase mustache looks up
  vm.summary = vm.summary || "";
}
