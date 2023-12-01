using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using SlipeServer.Server.Elements;
using System;
using System.Linq;

namespace SlipeServer.Server.TestTools.FluentAssertions;

public class ElementAssertionsBase<T> : ObjectAssertions<T, ElementAssertionsBase<T>> where T : Element
{
    public ElementAssertionsBase(T element) : base(element)
    {
    }

    protected void AssertPropertyEquality<U>(Func<T, U> propertySelector, U expected, string propertyName, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion.BecauseOf(because, becauseArgs)
            .ForCondition(propertySelector(Subject).Equals(expected))
            .FailWith($"Expected {propertyName} to be {{0}}{because}, but found {{1}}.", expected, propertySelector(Subject));
    }

    protected void AssertPropertyEquality(Func<T, byte[]> propertySelector, byte[] expected, string propertyName, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion.BecauseOf(because, becauseArgs)
            .ForCondition(propertySelector(Subject).SequenceEqual(expected))
            .FailWith($"Expected {propertyName} to be {{0}}{because}, but found {{1}}.", expected, propertySelector(Subject));
    }

    protected void AssertPropertyEquality(Func<T, float[]> propertySelector, float[] expected, string propertyName, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion.BecauseOf(because, becauseArgs)
            .ForCondition(propertySelector(Subject).SequenceEqual(expected))
            .FailWith($"Expected {propertyName} to be {{0}}{because}, but found {{1}}.", expected, propertySelector(Subject));
    }

    public virtual void BeEquivalentTo(T element, string because = "", params object[] becauseArgs)
    {
        AssertPropertyEquality(e => e.ElementType, element.ElementType, "ElementType", because, becauseArgs);
        AssertPropertyEquality(e => e.Name, element.Name, "Name", because, becauseArgs);
        AssertPropertyEquality(e => e.Position, element.Position, "Position", because, becauseArgs);
        AssertPropertyEquality(e => e.Rotation, element.Rotation, "Rotation", because, becauseArgs);
        AssertPropertyEquality(e => e.Velocity, element.Velocity, "Velocity", because, becauseArgs);
        AssertPropertyEquality(e => e.TurnVelocity, element.TurnVelocity, "TurnVelocity", because, becauseArgs);
        AssertPropertyEquality(e => (int)e.Interior, element.Interior, "Interior", because, becauseArgs);
        AssertPropertyEquality(e => (int)e.Dimension, element.Dimension, "Dimension", because, becauseArgs);
        AssertPropertyEquality(e => (int)e.Alpha, element.Alpha, "Alpha", because, becauseArgs);
        AssertPropertyEquality(e => e.AreCollisionsEnabled, element.AreCollisionsEnabled, "AreCollisionsEnabled", because, becauseArgs);
        AssertPropertyEquality(e => e.IsCallPropagationEnabled, element.IsCallPropagationEnabled, "IsCallPropagationEnabled", because, becauseArgs);
        AssertPropertyEquality(e => e.IsFrozen, element.IsFrozen, "IsFrozen", because, becauseArgs);
        AssertPropertyEquality(e => e.IsDestroyed, element.IsDestroyed, "IsDestroyed", because, becauseArgs);
    }
}

public class ElementAssertions : ElementAssertionsBase<Element>
{
    public ElementAssertions(Element element) : base(element)
    {
    }
}
