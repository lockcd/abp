# Creating the Initial Solution

````json
//[doc-nav]
{
  "Previous": {
    "Name": "Overview",
    "Path": "tutorials/modular-crm/index"
  },
  "Next": {
    "Name": "Setting Up the Catalog Module",
    "Path": "tutorials/modular-crm/part-02"
  }
}
````

Follow the *[Get Started](../../get-started/single-layer-web-application.md)* guide to create a single layer web application with the following configuration:

* **Solution name**: `ModularCrm`
* **UI Framework**: ASP.NET Core MVC / Razor Pages
* **Database Provider**: Entity Framework Core

You can select the other options based on your preference but at the **Modularity** step, check the _Setup as a modular solution_ option and add a new **Standard Module** named `ModularCrm.Catalog`:

![](./images/modular-crm-wizard-modularity-step.png)

Since modularity is a key aspect of the ABP Framework, it provides an option to create a modular system from the beginning. Here, you're creating a `ModularCrm.Catalog` module and setting it as a "Standard Module" (a module template similar to the DDD module but without the domain layer). This will create four projects (-depends on the options you selected-):
- `ModularCrm.Catalog`: The main module project
- `ModularCrm.Catalog.Contracts`: Contains service interfaces and DTOs
- `ModularCrm.Catalog.Tests`: Unit and integration tests (since we selected the _Include Test_ option)
- `ModularCrm.Catalog.UI`: Contains UI components for the module

> **Note:** This tutorial will guide you through creating two modules: `Catalog` and `Ordering`. While you just created the `Catalog` module in the _Modularity_ step, you could also create the `Ordering` module at this stage. However, you'll create the `Ordering` module in subsequent parts to better demonstrate ABP Studio's module management capabilities and to simulate a more realistic development workflow where modules are typically added incrementally as the application evolves.

This approach allows you to start with a modular architecture right from the beginning, making it easier to maintain and extend the application as it grows. ABP Studio automatically creates the module projects and places them under the `modules` folder for you, and later on you can add more modules to the solution.

> **Please complete the [Get Started](../../get-started/single-layer-web-application.md) guide and run the web application before going further.**

The initial solution structure should be like the following in ABP Studio's *[Solution Explorer](../../studio/solution-explorer.md)*:

![solution-explorer-modular-crm-initial-with-modules](images/solution-explorer-modular-crm-initial-with-modules.png)

Initially, you see a `ModularCrm` solution, a `ModularCrm` module under that solution (our main single layer application), and a `modules` folder that contains the `ModularCrm.Catalog` module.

> An ABP Studio module is typically a .NET solution and an ABP Studio solution is an umbrella concept for multiple .NET Solutions (see the [concepts](../../studio/concepts.md) document for more).

The `ModularCrm` module is the core of your application, built as a single-layer ASP.NET Core Web application. On the other hand, the `ModularCrm.Catalog` module is a standard module template, consist of 4 layers and used to implement the catalog features.

## Summary

We've created the initial single layer monolith solution. In the next part, we will learn how to create a new application module and install it to the main application.
