using System;
using System.Collections.Generic;

//----------------------------LAB 1-----------------------------------------------
// Фабричний метод та Абстрактна фабрика
public abstract class Dish
{
    public abstract string Prepare();
    // ДОДАЙТЕ ЦЕЙ АБСТРАКТНИЙ МЕТОД:
    public abstract void Accept(IDishVisitor visitor); 
}

// В класі Pizza
public class Pizza : Dish
{
    public override string Prepare() => "Готується піца з інгредієнтами.";
    // ДОДАЙТЕ РЕАЛІЗАЦІЮ МЕТОДУ Accept:
    public override void Accept(IDishVisitor visitor)
    {
        visitor.VisitPizza(this);
    }
}

// В класі Pasta
public class Pasta : Dish
{
    public override string Prepare() => "Готується паста з соусом.";
    // ДОДАЙТЕ РЕАЛІЗАЦІЮ МЕТОДУ Accept:
    public override void Accept(IDishVisitor visitor)
    {
        visitor.VisitPasta(this);
    }
}

abstract class DishFactory
{
    public abstract Dish CreateDish();
}

class PizzaFactory : DishFactory
{
    public override Dish CreateDish() => new Pizza();
}

class PastaFactory : DishFactory
{
    public override Dish CreateDish() => new Pasta();
}

//----------------------------LAB 2-----------------------------------------------
// Прототип
public interface ICloneableOrder
{
    Order Clone();
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

    public void AddDish(string dish) => _order.AddDish(dish);

    public Order Build() => _order;
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

//----------------------------LAB 3-----------------------------------------------
// Стратегія
public interface IPricingStrategy
{
    decimal CalculatePrice(Order order);
}

public class RegularPricing : IPricingStrategy
{
    public decimal CalculatePrice(Order order) => order.BasePrice;
}

public class DiscountPricing : IPricingStrategy
{
    public decimal CalculatePrice(Order order) => order.BasePrice * 0.9m;
}

public class PremiumPricing : IPricingStrategy
{
    public decimal CalculatePrice(Order order) => order.BasePrice * 1.2m;
}

// Спостерігач
public interface IOrderObserver
{
    void Update(Order order);
}

public class Waiter : IOrderObserver
{
    public void Update(Order order) => Console.WriteLine("Офіціант отримав оновлення: Нове замовлення!");
}

public class Chef : IOrderObserver
{
    public void Update(Order order) => Console.WriteLine("Шеф-кухар отримав оновлення: Нове замовлення на кухні!");
}

public class OrderNotifier
{
    private List<IOrderObserver> _observers = new List<IOrderObserver>();

    public void Attach(IOrderObserver observer) => _observers.Add(observer);
    public void Detach(IOrderObserver observer) => _observers.Remove(observer);
    public void Notify(Order order)
    {
        foreach (var observer in _observers)
        {
            observer.Update(order);
        }
    }
}

// Команда
public interface ICommand
{
    void Execute();
    void Undo();
}

public class AddDishCommand : ICommand
{
    private Order _order;
    private string _dish;

    public AddDishCommand(Order order, string dish)
    {
        _order = order;
        _dish = dish;
    }

    public void Execute()
    {
        _order.AddDish(_dish);
        Console.WriteLine($"Страва {_dish} додана до замовлення.");
    }

    public void Undo()
    {
        _order.RemoveDish(_dish);
        Console.WriteLine($"Страва {_dish} видалена з замовлення.");
    }
}

public class Order : ICloneableOrder
{
    public decimal BasePrice { get; private set; } = 100; // Залишаємо як є
    public List<string> Dishes { get; private set; } = new List<string>();
    public IPricingStrategy PricingStrategy { get; set; }
    public OrderNotifier Notifier { get; private set; } = new OrderNotifier();

    public void AddDish(string dish)
    {
        Dishes.Add(dish);
        Notifier.Notify(this);
    }

    public void RemoveDish(string dish)
    {
        Dishes.Remove(dish);
        Notifier.Notify(this);
    }

    public decimal GetFinalPrice() => PricingStrategy?.CalculatePrice(this) ?? BasePrice;

    public Order Clone()
    {
        Order copy = new Order { PricingStrategy = this.PricingStrategy, BasePrice = this.BasePrice }; // Копіюємо BasePrice
        copy.Dishes.AddRange(this.Dishes);
        return copy;
    }

    public void ShowOrder()
    {
        Console.WriteLine($"Order contains: {string.Join(", ", Dishes)}. Base Price: {BasePrice}. Final Price: {GetFinalPrice()}");
    }

    // ДОДАЙТЕ ЦІ МЕТОДИ ДЛЯ MEMENTO:
    public OrderMemento SaveState()
    {
        // Створюємо новий Memento з поточним станом
        return new OrderMemento(new List<string>(this.Dishes), this.PricingStrategy, this.BasePrice);
    }

    public void RestoreState(OrderMemento memento)
    {
        // Відновлюємо стан з Memento
        // Важливо створити нову копію списку, щоб уникнути спільного використання списків
        this.Dishes = new List<string>(memento.SavedDishes);
        this.PricingStrategy = memento.SavedPricingStrategy;
        this.BasePrice = memento.SavedBasePrice; // BasePrice має private set, тому його можна змінити зсередини класу

        Console.WriteLine("Стан Замовлення відновлено з Memento.");
        Notifier.Notify(this); // Повідомляємо спостерігачів про зміну стану
    }
}

//---------------------------- АДАПТЕР -----------------------------------
public interface IPaymentSystem
{
    void ProcessPayment(decimal amount);
}

public class ExternalPaymentService
{
    public void MakePayment(double amount)
    {
        Console.WriteLine($"Оплата у зовнішньому сервісі на суму {amount} грн виконана.");
    }
}

public class PaymentAdapter : IPaymentSystem
{
    private ExternalPaymentService _externalService = new ExternalPaymentService();
    
    public void ProcessPayment(decimal amount)
    {
        _externalService.MakePayment((double)amount);
    }
}

//---------------------------- ФАСАД ------------------------------------
public class RestaurantFacade
{
    private Order _order;
    private IPaymentSystem _paymentSystem;
    
    public RestaurantFacade()
    {
        _order = new Order();
        _paymentSystem = new PaymentAdapter();
    }
    
    public void PlaceOrder(string dish)
    {
        _order.AddDish(dish);
        Console.WriteLine("Замовлення додано.");
    }
    
    public void PayOrder()
    {
        decimal amount = _order.GetFinalPrice();
        _paymentSystem.ProcessPayment(amount);
    }
}

//---------------------------- СТАН -------------------------------------
public interface IOrderState
{
    void NextState(OrderContext context);
    void PrintStatus();
}

public class PendingState : IOrderState
{
    public void NextState(OrderContext context)
    {
        context.SetState(new CookingState());
    }
    
    public void PrintStatus()
    {
        Console.WriteLine("Замовлення очікує на приготування.");
    }
}

public class CookingState : IOrderState
{
    public void NextState(OrderContext context)
    {
        context.SetState(new ReadyState());
    }
    
    public void PrintStatus()
    {
        Console.WriteLine("Замовлення готується.");
    }
}

public class ReadyState : IOrderState
{
    public void NextState(OrderContext context)
    {
        context.SetState(new DeliveredState());
    }
    
    public void PrintStatus()
    {
        Console.WriteLine("Замовлення готове до видачі.");
    }
}

public class DeliveredState : IOrderState
{
    public void NextState(OrderContext context)
    {
        Console.WriteLine("Замовлення вже доставлено.");
    }
    
    public void PrintStatus()
    {
        Console.WriteLine("Замовлення доставлено.");
    }
}

public class OrderContext
{
    private IOrderState _state;
    
    public OrderContext()
    {
        _state = new PendingState();
    }
    
    public void SetState(IOrderState state)
    {
        _state = state;
    }
    
    public void NextState()
    {
        _state.NextState(this);
    }
    
    public void PrintStatus()
    {
        _state.PrintStatus();
    }
}

//--------------------------------- LAB 5
public interface IOrderIterator
{
    bool HasNext();
    Order Next();
}

public class OrderIterator : IOrderIterator
{
    private readonly List<Order> _orders;
    private int _position = 0;

    public OrderIterator(List<Order> orders)
    {
        _orders = orders;
    }

    public bool HasNext() => _position < _orders.Count;

    public Order Next() => _orders[_position++];
}

public class OrderCollection
{
    private List<Order> _orders = new List<Order>();

    public void AddOrder(Order order) => _orders.Add(order);
    public IOrderIterator CreateIterator() => new OrderIterator(_orders);
}

public abstract class ComplaintHandler
{
    protected ComplaintHandler _nextHandler;

    public void SetNext(ComplaintHandler nextHandler) => _nextHandler = nextHandler;

    public abstract void HandleComplaint(string complaint);
}

public class Manager : ComplaintHandler
{
    public override void HandleComplaint(string complaint)
    {
        if (complaint.ToLower().Contains("запізн"))
        {
            Console.WriteLine("Менеджер: Приношу вибачення за запізнення. Ми виправимо ситуацію.");
        }
        else if (_nextHandler != null)
        {
            _nextHandler.HandleComplaint(complaint);
        }
        else
        {
            Console.WriteLine("Менеджер: Скаргу не вдалося обробити.");
        }
    }
}

public class ChefHandler : ComplaintHandler
{
    public override void HandleComplaint(string complaint)
    {
        if (complaint.ToLower().Contains("страва") || complaint.ToLower().Contains("їжа"))
        {
            Console.WriteLine("Шеф-кухар: Перепрошую. Я приготую нову страву.");
        }
        else if (_nextHandler != null)
        {
            _nextHandler.HandleComplaint(complaint);
        }
        else
        {
            Console.WriteLine("Шеф-кухар: Скаргу не вдалося обробити.");
        }
    }
}


public class DiscountHandler : ComplaintHandler
{
    public override void HandleComplaint(string complaint)
    {
        if (complaint.ToLower().Contains("знижк") || complaint.ToLower().Contains("дешевше") || complaint.ToLower().Contains("ціна"))
        {
            Console.WriteLine("Бухгалтерія: Вам буде надано знижку 10% на наступне замовлення.");
        }
        else if (_nextHandler != null)
        {
            _nextHandler.HandleComplaint(complaint);
        }
        else
        {
            Console.WriteLine("Бухгалтерія: Скаргу не вдалося обробити.");
        }
    }
}

// ------------------ INTERPRETER --------------------------
public interface IExpression
{
    void Interpret(Order order);
}

public class AddDishExpression : IExpression
{
    private string _dish;
    public AddDishExpression(string dish) => _dish = dish;

    public void Interpret(Order order)
    {
        order.AddDish(_dish);
        Console.WriteLine($"[Interpreter] Додано страву: {_dish}");
    }
}

public class RemoveDishExpression : IExpression
{
    private string _dish;
    public RemoveDishExpression(string dish) => _dish = dish;

    public void Interpret(Order order)
    {
        order.RemoveDish(_dish);
        Console.WriteLine($"[Interpreter] Видалено страву: {_dish}");
    }
}

public class InterpreterContext
{
    public static IExpression Parse(string input)
    {
        var parts = input.Split(' ', 2);
        if (parts.Length != 2) return null;

        string command = parts[0].ToLower();
        string dish = parts[1];

        return command switch
        {
            "додати" => new AddDishExpression(dish),
            "прибрати" => new RemoveDishExpression(dish),
            _ => null
        };
    }
}

// ------------------ MEDIATOR -----------------------------

public interface IMediator
{
    void Notify(object sender, string ev);
}

public class KitchenMediator : IMediator
{
    public WaiterMediator Waiter { get; set; }
    public ChefMediator Chef { get; set; }
    public Order Order { get; set; }

    public void Notify(object sender, string ev)
    {
        if (ev == "NewOrder")
        {
            Chef.CookDish(Order.Dishes);
        }
        else if (ev == "Ready")
        {
            Waiter.ServeOrder(Order.Dishes);
        }
    }
}

public class WaiterMediator
{
    private IMediator _mediator;
    public WaiterMediator(IMediator mediator) => _mediator = mediator;

    public void TakeOrder(Order order)
    {
        Console.WriteLine("[Mediator] Офіціант прийняв замовлення.");
        (_mediator as KitchenMediator).Order = order;
        _mediator.Notify(this, "NewOrder");
    }

    public void ServeOrder(List<string> dishes)
    {
        Console.WriteLine("[Mediator] Офіціант подає: " + string.Join(", ", dishes));
    }
}

public class ChefMediator
{
    private IMediator _mediator;
    public ChefMediator(IMediator mediator) => _mediator = mediator;

    public void CookDish(List<string> dishes)
    {
        Console.WriteLine("[Mediator] Шеф-кухар готує: " + string.Join(", ", dishes));
        _mediator.Notify(this, "Ready");
    }
}

//----------------------------LAB 7: MEMENTO--------------------------------------
// Memento: Зберігає стан об'єкта Order
public class OrderMemento
{
    public List<string> SavedDishes { get; }
    public IPricingStrategy SavedPricingStrategy { get; }
    public decimal SavedBasePrice { get; }

    public OrderMemento(List<string> dishes, IPricingStrategy pricingStrategy, decimal basePrice)
    {
        SavedDishes = new List<string>(dishes); 
        SavedPricingStrategy = pricingStrategy; // Стратегію можна передавати за посиланням, якщо вона незмінна
        SavedBasePrice = basePrice;
    }
}

// Caretaker: Відповідає за збереження та відновлення Memento
public class OrderCaretaker
{
    private Stack<OrderMemento> _mementos = new Stack<OrderMemento>();
    private Order _originator;

    public OrderCaretaker(Order originator)
    {
        _originator = originator;
    }

    public void Backup()
    {
        Console.WriteLine("Caretaker: Зберігаю поточний стан Замовлення...");
        _mementos.Push(_originator.SaveState());
    }

    public void Undo()
    {
        if (_mementos.Count == 0)
        {
            Console.WriteLine("Caretaker: Немає збережених станів для відновлення.");
            return;
        }

        var memento = _mementos.Pop();
        Console.WriteLine("Caretaker: Відновлюю Замовлення до попереднього стану...");
        _originator.RestoreState(memento);
    }

    public void ShowHistory()
    {
        if (_mementos.Count == 0)
        {
            Console.WriteLine("Caretaker: Історія порожня.");
            return;
        }
        Console.WriteLine("Caretaker: Історія збережених станів (останній зверху):");
        foreach (var memento in _mementos)
        {
            Console.WriteLine($" - Стан: {memento.SavedDishes.Count} страв(и), Базова ціна: {memento.SavedBasePrice}, Стратегія: {memento.SavedPricingStrategy?.GetType().Name ?? "None"}");
        }
    }
}

//----------------------------LAB 7: VISITOR--------------------------------------
// Інтерфейс Відвідувача
public interface IDishVisitor
{
    void VisitPizza(Pizza pizza);
    void VisitPasta(Pasta pasta);
}

// Конкретний Відвідувач: виводить інформацію про тип страви
public class DishInfoVisitor : IDishVisitor
{
    public void VisitPizza(Pizza pizza)
    {
        Console.WriteLine($"[Visitor] Інформація: Це Піца. Процес: {pizza.Prepare()}");
    }

    public void VisitPasta(Pasta pasta)
    {
        Console.WriteLine($"[Visitor] Інформація: Це Паста. Процес: {pasta.Prepare()}");
    }
}

public class CalorieCheckVisitor : IDishVisitor
{
    public void VisitPizza(Pizza pizza)
    {
        Console.WriteLine("[Visitor] Піца: приблизно 800 ккал.");
    }

    public void VisitPasta(Pasta pasta)
    {
        Console.WriteLine("[Visitor] Паста: приблизно 600 ккал.");
    }
}

class Program
{
    static void Main()
    {
        //------------------------LAB 1-----------------------------------
        DishFactory pizzaFactory = new PizzaFactory();
        DishFactory pastaFactory = new PastaFactory();
        
        Console.WriteLine(OrderDish(pizzaFactory));
        Console.WriteLine(OrderDish(pastaFactory));

        //----------------------LAB 2-------------------------------------
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

        //----------------------LAB 3-------------------------------------
        Order order = new Order();
        order.PricingStrategy = new DiscountPricing();

        Waiter waiter = new Waiter();
        Chef chef = new Chef();
        order.Notifier.Attach(waiter);
        order.Notifier.Attach(chef);

        ICommand addDish = new AddDishCommand(order, "Піца");
        addDish.Execute();
        Console.WriteLine($"Фінальна ціна: {order.GetFinalPrice()} грн");
        addDish.Undo();

        // Використання Адаптера
        IPaymentSystem payment = new PaymentAdapter();
        payment.ProcessPayment(250);
        
        // Використання Фасаду
        RestaurantFacade restaurant = new RestaurantFacade();
        restaurant.PlaceOrder("Піца");
        restaurant.PayOrder();
        
        // Використання Стану
        OrderContext orderc = new OrderContext();
        orderc.PrintStatus();
        orderc.NextState();
        orderc.PrintStatus();
        orderc.NextState();
        orderc.PrintStatus();
        orderc.NextState();
        orderc.PrintStatus();

        // ------------------- ІТЕРАТОР -------------------
        Console.WriteLine("--- Ітератор ---");
        OrderCollection collection = new OrderCollection();
        collection.AddOrder(originalOrder);
        collection.AddOrder(clonedOrder);
        collection.AddOrder(customOrder);

        IOrderIterator iterator = collection.CreateIterator();
        while (iterator.HasNext())
        {
            Order current = iterator.Next();
            current.ShowOrder();
        }

        // ------------------- ЛАНЦЮЖОК ОБОВ'ЯЗКІВ -------------------
        Console.WriteLine("--- Ланцюжок обов'язків ---");
        ComplaintHandler managerHandler = new Manager();
        ComplaintHandler chefHandler = new ChefHandler();
        ComplaintHandler discountHandler = new DiscountHandler();

        managerHandler.SetNext(chefHandler);
        chefHandler.SetNext(discountHandler);

        managerHandler.HandleComplaint("Моє замовлення запізнилося!");
        managerHandler.HandleComplaint("Моя страва була холодна!");
        managerHandler.HandleComplaint("Я хочу знижку!");



        // ------------------- INTERPRETER -------------------
        Console.WriteLine("--- Interpreter ---");
        Order interpreterOrder = new Order();

        string[] commands = {
            "додати бургер",
            "додати салат",
            "прибрати салат"
        };

        foreach (string cmd in commands)
        {
            IExpression expr = InterpreterContext.Parse(cmd);
            expr?.Interpret(interpreterOrder);
        }
        interpreterOrder.ShowOrder();

        // ------------------- MEDIATOR ----------------------
        Console.WriteLine("--- Mediator ---");
        Order mediatorOrder = new Order();
        mediatorOrder.AddDish("Паста");
        mediatorOrder.AddDish("Суп");

        KitchenMediator mediator = new KitchenMediator();
        WaiterMediator waiterMediator = new WaiterMediator(mediator);
        ChefMediator chefMediator = new ChefMediator(mediator);

        mediator.Waiter = waiterMediator;
        mediator.Chef = chefMediator;

        waiterMediator.TakeOrder(mediatorOrder);

        Console.WriteLine("\n\n----------------------LAB 7: MEMENTO------------------------------");
        Order orderForMemento = new Order();
        OrderCaretaker caretaker = new OrderCaretaker(orderForMemento);

        Console.WriteLine("Початковий стан Замовлення:");
        orderForMemento.ShowOrder();
        caretaker.Backup();

        orderForMemento.AddDish("Салат Цезар");
        orderForMemento.PricingStrategy = new PremiumPricing();
        Console.WriteLine("\nЗамовлення після додавання 'Салат Цезар' та зміни стратегії:");
        orderForMemento.ShowOrder();
        caretaker.Backup();

        orderForMemento.AddDish("Сік апельсиновий");
        Console.WriteLine("\nЗамовлення після додавання 'Сік апельсиновий':");
        orderForMemento.ShowOrder();
     

        Console.WriteLine("\n--- Відновлення станів ---");
        caretaker.Undo(); 
        Console.WriteLine("Замовлення після першого Undo (повинен бути 'Салат Цезар', PremiumPricing):");
        orderForMemento.ShowOrder();

        caretaker.Undo(); 
        Console.WriteLine("\nЗамовлення після другого Undo (початковий стан):");
        orderForMemento.ShowOrder();

        caretaker.ShowHistory();

        Console.WriteLine("\n\n----------------------LAB 7: VISITOR------------------------------");
        List<Dish> dishesToVisit = new List<Dish>
        {
            pizzaFactory.CreateDish(), 
            pastaFactory.CreateDish(),
            new Pizza() 
        };

        IDishVisitor infoVisitor = new DishInfoVisitor();
        IDishVisitor calorieVisitor = new CalorieCheckVisitor();

        Console.WriteLine("\n--- Обхід Відвідувачем DishInfoVisitor ---");
        foreach (var dishItem in dishesToVisit)
        {
            dishItem.Accept(infoVisitor);
        }

        Console.WriteLine("\n--- Обхід Відвідувачем CalorieCheckVisitor ---");
        foreach (var dishItem in dishesToVisit)
        {
            dishItem.Accept(calorieVisitor);
        }

    }

    //--------------------LAB 1------------------------------
    static string OrderDish(DishFactory factory)
    {
        Dish dish = factory.CreateDish();
        return dish.Prepare();
    }

    
} 