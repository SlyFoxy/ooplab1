using System;
using System.Collections.Generic;
using System.Linq; // Added for Any() in Facade and Stack in Memento
using System.Threading; // Added for Thread.Sleep in Proxy

#region LAB 1: Creational - Factory Method, Abstract Factory

//----------------------------LAB 1 (Фабричний метод та Абстрактна фабрика)-----------------------------------------------
public abstract class Dish
{
    public abstract string Prepare();
    public abstract void Accept(IDishVisitor visitor); // For LAB 7: Visitor
}

public class Pizza : Dish
{
    public override string Prepare() => "Готується піца з інгредієнтами.";
    public override void Accept(IDishVisitor visitor)
    {
        visitor.VisitPizza(this);
    }
}

public class Pasta : Dish
{
    public override string Prepare() => "Готується паста з соусом.";
    public override void Accept(IDishVisitor visitor)
    {
        visitor.VisitPasta(this);
    }
}

public abstract class DishFactory
{
    public abstract Dish CreateDish();
}

public class PizzaFactory : DishFactory
{
    public override Dish CreateDish() => new Pizza();
}

public class PastaFactory : DishFactory
{
    public override Dish CreateDish() => new Pasta();
}

#endregion

#region LAB 2: Creational - Prototype, Builder

//----------------------------LAB 2 (Prototype, Builder)-----------------------------------------------
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

    public Order Build() 
    {
        Order builtOrder = _order;
        _order = new Order(); 
        return builtOrder;
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

#endregion

#region LAB 3: Behavioral - Strategy, Observer, Command

//----------------------------LAB 3 (Strategy, Observer, Command)-----------------------------------------------
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

#endregion

#region Core Class: Order (Used across multiple labs)

public class Order : ICloneableOrder
{
    public decimal BasePrice { get; private set; } = 100;
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
        Order copy = new Order { PricingStrategy = this.PricingStrategy, BasePrice = this.BasePrice };
        copy.Dishes.AddRange(this.Dishes);
        return copy;
    }

    public void ShowOrder()
    {
        Console.WriteLine($"Order contains: {(Dishes.Any() ? string.Join(", ", Dishes) : "порожньо")}. Base Price: {BasePrice:C}. Final Price: {GetFinalPrice():C}");
    }

    // For LAB 7: Memento
    public OrderMemento SaveState()
    {
        return new OrderMemento(new List<string>(this.Dishes), this.PricingStrategy, this.BasePrice);
    }

    public void RestoreState(OrderMemento memento)
    {
        this.Dishes = new List<string>(memento.SavedDishes);
        this.PricingStrategy = memento.SavedPricingStrategy;
        this.BasePrice = memento.SavedBasePrice;
        Console.WriteLine("Стан Замовлення відновлено з Memento.");
        Notifier.Notify(this);
    }
}

#endregion

#region Structural Pattern (Used for Facade in LAB 8, but defined early) - Adapter

//---------------------------- АДАПТЕР (Використовується в Фасаді LAB 8) -----------------------------------
public interface IPaymentSystem
{
    void ProcessPayment(decimal amount);
}

public class ExternalPaymentService
{
    public void MakePayment(double amount)
    {
        Console.WriteLine($"Оплата у зовнішньому сервісі на суму {amount:C} виконана.");
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

#endregion

#region LAB 8: Facade (part of Structural patterns)

//---------------------------- ФАСАД (LAB 8) ------------------------------------
public class RestaurantFacade
{
    private Order _currentOrder;
    private IPaymentSystem _paymentSystem;
    private KitchenMediator _kitchenMediator; // For complex interaction, LAB 6
    private WaiterMediator _waiterMediator;   // For complex interaction, LAB 6
    private ChefMediator _chefForMediator;    // For complex interaction, LAB 6

    public RestaurantFacade()
    {
        _currentOrder = new Order();
        _paymentSystem = new PaymentAdapter();


        _kitchenMediator = new KitchenMediator();
        _waiterMediator = new WaiterMediator(_kitchenMediator);
        _chefForMediator = new ChefMediator(_kitchenMediator);
        _kitchenMediator.Waiter = _waiterMediator;
        _kitchenMediator.Chef = _chefForMediator;
    }
    
    public void AddDishToOrder(string dish)
    {
        _currentOrder.AddDish(dish);
        Console.WriteLine($"[Facade] Страва '{dish}' додана до поточного замовлення.");
    }

    public void ShowCurrentOrderDetails()
    {
        Console.WriteLine("[Facade] Деталі поточного замовлення:");
        _currentOrder.ShowOrder();
    }
    
    public void PlaceAndPayOrder()
    {
        if (!_currentOrder.Dishes.Any())
        {
            Console.WriteLine("[Facade] Замовлення порожнє. Нічого оплачувати.");
            return;
        }

        Console.WriteLine("[Facade] Розміщення та оплата замовлення..."); 

        decimal amount = _currentOrder.GetFinalPrice();
        _paymentSystem.ProcessPayment(amount);
        Console.WriteLine($"[Facade] Замовлення на суму {amount:C} успішно оплачено.");
        _currentOrder = new Order(); // Reset for next customer
    }

    public void OrderComboMealAndPay()
    {
        Console.WriteLine("[Facade] Замовлення Комбо-меню...");
        AddDishToOrder("Бургер Комбо");
        AddDishToOrder("Картопля Фрі Комбо");
        AddDishToOrder("Напій Комбо");
        _currentOrder.PricingStrategy = new DiscountPricing(); 
        ShowCurrentOrderDetails();
        PlaceAndPayOrder();
    }
}

#endregion

#region LAB 5: Behavioral - State, Iterator, Chain of Responsibility

//---------------------------- СТАН (LAB 5) -------------------------------------
public interface IOrderState
{
    void NextState(OrderContext context);
    void PrintStatus();
}

public class PendingState : IOrderState
{
    public void NextState(OrderContext context) => context.SetState(new CookingState());
    public void PrintStatus() => Console.WriteLine("Стан замовлення: Очікує на приготування.");
}

public class CookingState : IOrderState
{
    public void NextState(OrderContext context) => context.SetState(new ReadyState());
    public void PrintStatus() => Console.WriteLine("Стан замовлення: Готується.");
}

public class ReadyState : IOrderState
{
    public void NextState(OrderContext context) => context.SetState(new DeliveredState());
    public void PrintStatus() => Console.WriteLine("Стан замовлення: Готове до видачі.");
}

public class DeliveredState : IOrderState
{
    public void NextState(OrderContext context) => Console.WriteLine("Замовлення вже доставлено.");
    public void PrintStatus() => Console.WriteLine("Стан замовлення: Доставлено.");
}

public class OrderContext
{
    private IOrderState _state;
    
    public OrderContext()
    {
        _state = new PendingState();
    }
    
    public void SetState(IOrderState state) => _state = state;
    public void NextState() => _state.NextState(this);
    public void PrintStatus() => _state.PrintStatus();
}

//--------------------------------- ІТЕРАТОР (LAB 5) ---------------------------------
public interface IOrderIterator
{
    bool HasNext();
    Order Next();
}

public class OrderIterator : IOrderIterator
{
    private readonly List<Order> _orders;
    private int _position = 0;

    public OrderIterator(List<Order> orders) => _orders = orders;
    public bool HasNext() => _position < _orders.Count;
    public Order Next() => _orders[_position++];
}

public class OrderCollection
{
    private List<Order> _orders = new List<Order>();

    public void AddOrder(Order order) => _orders.Add(order);
    public IOrderIterator CreateIterator() => new OrderIterator(_orders);
}

//--------------------------------- ЛАНЦЮЖОК ОБОВ'ЯЗКІВ (LAB 5) --------------------
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

#endregion

#region LAB 6: Behavioral - Interpreter, Mediator

// ------------------ INTERPRETER (LAB 6) --------------------------
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
        var parts = input.Split(new[] { ' ' }, 2);
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

// ------------------ MEDIATOR (LAB 6) -----------------------------
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
        if (ev == "NewOrder" && sender is WaiterMediator)
        {
            Console.WriteLine("[KitchenMediator] Отримано NewOrder від Офіціанта. Інформую Шефа.");
            Chef?.CookDish(Order.Dishes);
        }
        else if (ev == "Ready" && sender is ChefMediator)
        {
            Console.WriteLine("[KitchenMediator] Отримано Ready від Шефа. Інформую Офіціанта.");
            Waiter?.ServeOrder(Order.Dishes);
        }
    }
}

public class WaiterMediator
{
    private IMediator _mediator;
    public WaiterMediator(IMediator mediator) => _mediator = mediator;

    public void TakeOrder(Order order)
    {
        Console.WriteLine("[WaiterColleague] Офіціант прийняв замовлення.");
        if (_mediator is KitchenMediator km) km.Order = order; 
        _mediator.Notify(this, "NewOrder");
    }

    public void ServeOrder(List<string> dishes)
    {
        Console.WriteLine("[WaiterColleague] Офіціант подає: " + string.Join(", ", dishes));
    }
}

public class ChefMediator
{
    private IMediator _mediator;
    public ChefMediator(IMediator mediator) => _mediator = mediator;

    public void CookDish(List<string> dishes)
    {
        Console.WriteLine("[ChefColleague] Шеф-кухар готує: " + string.Join(", ", dishes));
        Thread.Sleep(500);
        _mediator.Notify(this, "Ready");
    }
}

#endregion

#region LAB 7: Behavioral - Memento, Visitor

//----------------------------MEMENTO (LAB 7)--------------------------------------
public class OrderMemento
{
    public List<string> SavedDishes { get; }
    public IPricingStrategy SavedPricingStrategy { get; }
    public decimal SavedBasePrice { get; }

    public OrderMemento(List<string> dishes, IPricingStrategy pricingStrategy, decimal basePrice)
    {
        SavedDishes = new List<string>(dishes); 
        SavedPricingStrategy = pricingStrategy;
        SavedBasePrice = basePrice;
    }
}

public class OrderCaretaker
{
    private Stack<OrderMemento> _mementos = new Stack<OrderMemento>();
    private Order _originator;

    public OrderCaretaker(Order originator) => _originator = originator;

    public void Backup()
    {
        Console.WriteLine("Caretaker: Зберігаю поточний стан Замовлення...");
        _mementos.Push(_originator.SaveState());
    }

    public void Undo()
    {
        if (!_mementos.Any())
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
        if (!_mementos.Any())
        {
            Console.WriteLine("Caretaker: Історія порожня.");
            return;
        }
        Console.WriteLine("Caretaker: Історія збережених станів (останній зверху):");
        foreach (var memento in _mementos.Reverse())
        {
            Console.WriteLine($" - Стан: {memento.SavedDishes.Count} страв(и), Базова ціна: {memento.SavedBasePrice:C}, Стратегія: {memento.SavedPricingStrategy?.GetType().Name ?? "None"}");
        }
    }
}

//----------------------------VISITOR (LAB 7)--------------------------------------
public interface IDishVisitor
{
    void VisitPizza(Pizza pizza);
    void VisitPasta(Pasta pasta);
}

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
    public void VisitPizza(Pizza pizza) => Console.WriteLine("[Visitor] Піца: приблизно 800 ккал.");
    public void VisitPasta(Pasta pasta) => Console.WriteLine("[Visitor] Паста: приблизно 600 ккал.");
}

#endregion

#region LAB 8: Structural - Proxy, Bridge (Facade is also LAB 8)

//----------------------------PROXY (LAB 8)--------------------------------------
public interface ICookingService
{
    Dish PrepareSpecialDish(string dishName);
}

public class RealCookingService : ICookingService
{
    public Dish PrepareSpecialDish(string dishName)
    {
        Console.WriteLine($"[RealCookingService] Готується дуже особлива страва: {dishName}. Це займає багато часу і ресурсів...");
        Thread.Sleep(1000); 
        if (dishName.ToLower() == "секретна страва від шефа")
        {
            var secretPizza = new Pizza(); 
            Console.WriteLine($"[RealCookingService] {dishName} готова!");
            return secretPizza;
        }
        Console.WriteLine($"[RealCookingService] Страва {dishName} не є нашою спеціалізацією сьогодні.");
        return null;
    }
}

public class CookingServiceProxy : ICookingService
{
    private RealCookingService _realService;
    private List<string> _allowedUsersForSecretDish;

    public CookingServiceProxy(List<string> allowedUsers)
    {
        _allowedUsersForSecretDish = allowedUsers ?? new List<string>();
    }

    public Dish PrepareSpecialDish(string dishName)
    {
        if (dishName.ToLower() == "секретна страва від шефа")
        {
            Console.Write("Введіть ваше ім'я для замовлення секретної страви: ");
            string currentUser = Console.ReadLine();

            if (CanUserOrderSecretDish(currentUser))
            {
                Console.WriteLine($"[Proxy] Користувач '{currentUser}' має доступ. Передаємо запит реальному сервісу.");
                if (_realService == null)
                {
                    Console.WriteLine("[Proxy] Ініціалізуємо реальний сервіс приготування...");
                    _realService = new RealCookingService();
                }
                return _realService.PrepareSpecialDish(dishName);
            }
            else
            {
                Console.WriteLine($"[Proxy] Доступ заборонено! Користувач '{currentUser}' не може замовити '{dishName}'.");
                return null;
            }
        }
        else
        {
            Console.WriteLine($"[Proxy] Запит на звичайну страву '{dishName}'. Передаємо реальному сервісу (якщо він потрібен).");
            if (_realService == null) _realService = new RealCookingService();
            return _realService.PrepareSpecialDish(dishName); 
        }
    }

    private bool CanUserOrderSecretDish(string userName)
    {
        return !string.IsNullOrWhiteSpace(userName) && 
               _allowedUsersForSecretDish.Contains(userName.Trim().ToLower());
    }
}

//----------------------------BRIDGE (LAB 8)--------------------------------------
public interface IDishServingStyle // Implementor
{
    void Serve(Dish dish);
    string GetServingDescription();
}

public class DineInServingStyle : IDishServingStyle // ConcreteImplementor A
{
    public void Serve(Dish dish) => Console.WriteLine($"[DineInServing] Страва '{dish.GetType().Name}' подається на красивій тарілці з приборами.");
    public string GetServingDescription() => "Подача в залі ресторану";
}

public class TakeAwayServingStyle : IDishServingStyle // ConcreteImplementor B
{
    public void Serve(Dish dish) => Console.WriteLine($"[TakeAwayServing] Страва '{dish.GetType().Name}' ретельно запакована в контейнер для виносу.");
    public string GetServingDescription() => "Запаковано на виніс";
}

public abstract class ServableDish // Abstraction
{
    protected Dish _dishType;
    protected IDishServingStyle _servingStyle;

    protected ServableDish(Dish dish, IDishServingStyle servingStyle)
    {
        _dishType = dish;
        _servingStyle = servingStyle;
    }

    public abstract void PresentDish();
    public void SetServingStyle(IDishServingStyle servingStyle)
    {
        _servingStyle = servingStyle;
        Console.WriteLine($"[ServableDish] Змінено стиль подачі на: {_servingStyle.GetServingDescription()}");
    }
}

public class PreparedPizza : ServableDish // RefinedAbstraction A
{
    public PreparedPizza(Pizza pizza, IDishServingStyle servingStyle) : base(pizza, servingStyle) { }

    public override void PresentDish()
    {
        Console.WriteLine($"[PreparedPizza] Готуємо до подачі піцу...");
        _dishType.Prepare();
        _servingStyle.Serve(_dishType);
    }
}

public class PreparedPasta : ServableDish // RefinedAbstraction B
{
    public PreparedPasta(Pasta pasta, IDishServingStyle servingStyle) : base(pasta, servingStyle) { }

    public override void PresentDish()
    {
        Console.WriteLine($"[PreparedPasta] Готуємо до подачі пасту...");
        _dishType.Prepare();
        _servingStyle.Serve(_dishType);
    }
}

#endregion

#region Main Program Logic

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        #region LAB 1 Demo
        Console.WriteLine("------------------------LAB 1: FACTORIES-----------------------------------");
        DishFactory pizzaFactory = new PizzaFactory();
        DishFactory pastaFactory = new PastaFactory();
        
        Console.WriteLine(OrderDish(pizzaFactory));
        Console.WriteLine(OrderDish(pastaFactory));
        #endregion

        #region LAB 2 Demo
        Console.WriteLine("\n----------------------LAB 2: PROTOTYPE & BUILDER--------------------------");
        Console.WriteLine("--- Prototype Pattern ---");
        Order originalOrder = new Order();
        originalOrder.AddDish("Pizza Original");
        originalOrder.AddDish("Pasta Original");
        originalOrder.ShowOrder();

        Order clonedOrder = originalOrder.Clone();
        clonedOrder.AddDish("Cola Cloned"); 
        Console.WriteLine("Cloned order:");
        clonedOrder.ShowOrder();
        Console.WriteLine("Original after cloning (should be unchanged):");
        originalOrder.ShowOrder();


        Console.WriteLine("\n--- Builder Pattern ---");
        IOrderBuilder builder = new OrderBuilder();
        builder.AddDish("Steak");
        builder.AddDish("Salad");
        Order customOrder = builder.Build();
        customOrder.ShowOrder();

        Director director = new Director();
        Order comboMeal = director.CreateComboMeal(new OrderBuilder());
        comboMeal.ShowOrder();
        #endregion

        #region LAB 3 Demo
        Console.WriteLine("\n----------------------LAB 3: STRATEGY, OBSERVER, COMMAND--------------------");
        Order orderForLab3 = new Order();
        orderForLab3.PricingStrategy = new DiscountPricing();

        Waiter waiterObserver = new Waiter();
        Chef chefObserver = new Chef();
        orderForLab3.Notifier.Attach(waiterObserver);
        orderForLab3.Notifier.Attach(chefObserver);

        ICommand addDishCmd = new AddDishCommand(orderForLab3, "Піца Команда");
        addDishCmd.Execute();
        Console.WriteLine($"Фінальна ціна: {orderForLab3.GetFinalPrice():C}");
        addDishCmd.Undo();
        orderForLab3.ShowOrder();
        #endregion

        #region Adapter Demo (early definition, used in LAB 8 Facade)
        Console.WriteLine("\n--- Adapter (Demonstrated early, used by Facade) ---");
        IPaymentSystem paymentSystemAdapter = new PaymentAdapter();
        paymentSystemAdapter.ProcessPayment(250.75m);
        #endregion
        
        #region LAB 5 Demo: State, Iterator, Chain of Responsibility
        Console.WriteLine("\n----------------------LAB 5: STATE, ITERATOR, CoR-------------------------");
        Console.WriteLine("--- State Pattern ---");
        OrderContext orderStateContext = new OrderContext();
        orderStateContext.PrintStatus();
        orderStateContext.NextState();
        orderStateContext.PrintStatus();
        orderStateContext.NextState();
        orderStateContext.PrintStatus();
        orderStateContext.NextState();
        orderStateContext.PrintStatus();

        Console.WriteLine("\n--- Iterator Pattern ---");
        OrderCollection orderCollection = new OrderCollection();
        orderCollection.AddOrder(originalOrder);
        orderCollection.AddOrder(clonedOrder);
        orderCollection.AddOrder(customOrder);

        IOrderIterator orderIterator = orderCollection.CreateIterator();
        while (orderIterator.HasNext())
        {
            Order currentIteratedOrder = orderIterator.Next();
            currentIteratedOrder.ShowOrder();
        }

        Console.WriteLine("\n--- Chain of Responsibility Pattern ---");
        ComplaintHandler managerCoR = new Manager(); 
        ComplaintHandler chefCoR = new ChefHandler(); 
        ComplaintHandler discountCoR = new DiscountHandler(); 

        managerCoR.SetNext(chefCoR);
        chefCoR.SetNext(discountCoR);

        managerCoR.HandleComplaint("Моє замовлення запізнилося!");
        managerCoR.HandleComplaint("Моя страва була холодна!");
        managerCoR.HandleComplaint("Я хочу знижку!");
        managerCoR.HandleComplaint("Невідома скарга."); 
        #endregion

        #region LAB 6 Demo: Interpreter, Mediator
        Console.WriteLine("\n----------------------LAB 6: INTERPRETER, MEDIATOR-------------------------");
        Console.WriteLine("--- Interpreter Pattern ---");
        Order interpreterOrder = new Order();
        string[] commandsToInterpret = {
            "додати Бургер Інтерпретер",
            "додати Салат Інтерпретер",
            "прибрати Салат Інтерпретер"
        };
        foreach (string cmdStr in commandsToInterpret)
        {
            IExpression expr = InterpreterContext.Parse(cmdStr);
            expr?.Interpret(interpreterOrder);
        }
        interpreterOrder.ShowOrder();

        Console.WriteLine("\n--- Mediator Pattern ---");
        Order mediatorOrder = new Order();
        mediatorOrder.AddDish("Паста Медіатор");
        mediatorOrder.AddDish("Суп Медіатор");

        KitchenMediator kitchenMediatorInstance = new KitchenMediator();
        WaiterMediator waiterColleague = new WaiterMediator(kitchenMediatorInstance); 
        ChefMediator chefColleague = new ChefMediator(kitchenMediatorInstance);     

        kitchenMediatorInstance.Waiter = waiterColleague;
        kitchenMediatorInstance.Chef = chefColleague;

        waiterColleague.TakeOrder(mediatorOrder);
        #endregion

        #region LAB 7 Demo: Memento, Visitor
        Console.WriteLine("\n\n----------------------LAB 7: MEMENTO, VISITOR------------------------------");
        Console.WriteLine("--- Memento Pattern ---");
        Order orderForMemento = new Order();
        OrderCaretaker caretaker = new OrderCaretaker(orderForMemento);

        Console.WriteLine("Початковий стан Замовлення:");
        orderForMemento.ShowOrder();
        caretaker.Backup(); 

        orderForMemento.AddDish("Салат Цезар Memento");
        orderForMemento.PricingStrategy = new PremiumPricing(); 
        Console.WriteLine("\nЗамовлення після додавання 'Салат Цезар' та зміни стратегії:");
        orderForMemento.ShowOrder();
        caretaker.Backup(); 

        orderForMemento.AddDish("Сік апельсиновий Memento");
        Console.WriteLine("\nЗамовлення після додавання 'Сік апельсиновий':");
        orderForMemento.ShowOrder();
     
        Console.WriteLine("\n--- Відновлення станів ---");
        caretaker.Undo(); 
        Console.WriteLine("Замовлення після першого Undo:");
        orderForMemento.ShowOrder();

        caretaker.Undo(); 
        Console.WriteLine("\nЗамовлення після другого Undo:");
        orderForMemento.ShowOrder();

        caretaker.ShowHistory();

        Console.WriteLine("\n\n--- Visitor Pattern ---");
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
        #endregion

        #region LAB 8 Demo: Facade, Proxy, Bridge
        Console.WriteLine("\n\n----------------------LAB 8: FACADE, PROXY, BRIDGE------------------------------");
        
        Console.WriteLine("\n--- Facade Pattern (Розширений приклад) ---");
        RestaurantFacade facade = new RestaurantFacade();
        facade.AddDishToOrder("Паста Карбонара Facade");
        facade.AddDishToOrder("Салат Грецький Facade");
        facade.ShowCurrentOrderDetails();
        facade.PlaceAndPayOrder();

        Console.WriteLine("\n--- Facade: Замовлення Комбо ---");
        facade.OrderComboMealAndPay();

        Console.WriteLine("\n\n--- Proxy Pattern ---");
        ICookingService cookingService = new CookingServiceProxy(new List<string> { "admin", "vip_client" });

        Console.WriteLine("\nСпроба замовити секретну страву (введіть 'guest' або щось інше):");
        Dish secretDish1 = cookingService.PrepareSpecialDish("секретна страва від шефа");
        if (secretDish1 != null) Console.WriteLine($"Отримано: {secretDish1.Prepare()}");
        
        Console.WriteLine("\nСпроба замовити секретну страву (введіть 'admin'):");
        Dish secretDish2 = cookingService.PrepareSpecialDish("секретна страва від шефа");
        if (secretDish2 != null) Console.WriteLine($"Отримано: {secretDish2.Prepare()}");

        Console.WriteLine("\nСпроба замовити іншу страву (не секретну):");
        Dish regularSpecialDish = cookingService.PrepareSpecialDish("Фірмовий стейк");
        if (regularSpecialDish != null) Console.WriteLine($"Отримано: {regularSpecialDish.Prepare()}");
        

        Console.WriteLine("\n\n--- Bridge Pattern ---");
        Pizza myBridgePizza = new Pizza(); 
        Pasta myBridgePasta = new Pasta(); 

        IDishServingStyle dineInStyle = new DineInServingStyle();
        IDishServingStyle takeAwayStyle = new TakeAwayServingStyle();

        ServableDish pizzaForRestaurant = new PreparedPizza(myBridgePizza, dineInStyle);
        Console.WriteLine("Подача піци в ресторані:");
        pizzaForRestaurant.PresentDish();

        ServableDish pastaForTakeAway = new PreparedPasta(myBridgePasta, takeAwayStyle);
        Console.WriteLine("\nПодача пасти на виніс:");
        pastaForTakeAway.PresentDish();
        
        Console.WriteLine("\nЗмінюємо стиль подачі піци на 'на виніс':");
        pizzaForRestaurant.SetServingStyle(takeAwayStyle);
        pizzaForRestaurant.PresentDish();
        #endregion

        Console.WriteLine("\n\nНатисніть будь-яку клавішу для виходу...");
        Console.ReadKey();
    }

    // Helper method for LAB 1 Demo
    static string OrderDish(DishFactory factory)
    {
        Dish dish = factory.CreateDish();
        return dish.Prepare();
    }
}

#endregion