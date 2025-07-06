using UnityEngine;


// created for Lesson 8

public abstract class Command
{

    public abstract bool isComplete { get; } // {get} makes it a read-only property
    // private set, public get makes it a property that can be set privately but read publicly
    public abstract void Execute(); // method to be implemented by derived classes


}
