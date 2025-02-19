using System;
using System.Collections.Generic;

//----------------------------LAB 1-----------------------------------------------
//----------------------------LAB 1-----------------------------------------------
//----------------------------LAB 1-----------------------------------------------
abstract class Dish
{
    public abstract string Prepare();
}

class Pizza : Dish
{
    public override string Prepare()
    {
        return "Готується піца з інгредієнтами.";
    }
}

class Pasta : Dish
{
    public override string Prepare()
    {
        return "Готується паста з соусом.";
    }
}

abstract class DishFactory
{
    public abstract Dish CreateDish();
}

class PizzaFactory : DishFactory
{
    public override Dish CreateDish()
    {
        return new Pizza();
    }
}

class PastaFactory : DishFactory
{
    public override Dish CreateDish()
    {
        return new Pasta();
    }
}

//------------------------------------LAB 2-------------------------------------------
//------------------------------------LAB 2-------------------------------------------
//------------------------------------LAB 2-------------------------------------------
// Прототип
public interface ICloneableOrder
{
    Order Clone();
}

public class Order : ICloneableOrder
{
    public List<string> Dishes { get; private set; } = new List<string>();

    public void AddDish(string dish)
    {
        Dishes.Add(dish);
    }

    public Order Clone()
    {
        Order copy = new Order();
        copy.Dishes = new List<string>(this.Dishes);
        return copy;
    }

    public void ShowOrder()
    {
        Console.WriteLine("Order contains: " + string.Join(", ", Dishes));
    }
}

// Будівельник
public interface IOrderBuilder
{
    void AddDish(string dish);
    Order Build();
}

public class OrderBuilder : IOrderBuilder
{
    private Order _order = new Order();
    
    public void AddDish(string dish)
    {
        _order.AddDish(dish);
    }

    public Order Build()
    {
        return _order;
    }
}

public class Director
{
    public Order CreateComboMeal(IOrderBuilder builder)
    {
        builder.AddDish("Burger");
        builder.AddDish("Fries");
        builder.AddDish("Cola");
        return builder.Build();
    }
}


class Program
{
    static void Main()
    {
        //------------------------LAB 1-----------------------------------
        //------------------------LAB 1-----------------------------------
        //------------------------LAB 1-----------------------------------
        DishFactory pizzaFactory = new PizzaFactory();
        DishFactory pastaFactory = new PastaFactory();
        
        Console.WriteLine(OrderDish(pizzaFactory));  // Готується піца з інгредієнтами.
        Console.WriteLine(OrderDish(pastaFactory));  // Готується паста з соусом.

        //----------------------LAB 2------------------------------------------
        //----------------------LAB 2------------------------------------------
        //----------------------LAB 2------------------------------------------
        Console.WriteLine("--- Prototype Pattern ---");
        Order originalOrder = new Order();
        originalOrder.AddDish("Pizza");
        originalOrder.AddDish("Pasta");
        
        Order clonedOrder = originalOrder.Clone();
        clonedOrder.ShowOrder();
        
        Console.WriteLine("--- Builder Pattern ---");
        IOrderBuilder builder = new OrderBuilder();
        builder.AddDish("Steak");
        builder.AddDish("Salad");
        
        Order customOrder = builder.Build();
        customOrder.ShowOrder();
        
        Director director = new Director();
        Order comboMeal = director.CreateComboMeal(new OrderBuilder());
        comboMeal.ShowOrder();
    }
    
    //--------------------LAB 1------------------------------
    static string OrderDish(DishFactory factory)
    {
        Dish dish = factory.CreateDish();
        return dish.Prepare();
    }
}
