using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roles
{
    // role declaration: a place holder with methods (behavior)
    // declared to populate any object, i.e. any object
    // willing to take this role
    public interface ISubject { }
    public interface IProxy { }
    public interface IContext
    {
        IState State { get; set; }
    }
    public interface IState { }
}
namespace LibrarySystem
{
    using Roles;
    public class Services : IProxy
    {
        public Services() { }
    }
    public class Administration { }
    public class ResourceCollections { }
    public class Resort
    {
        // properties
        public Services Services { get; set; }
        public Administration Administration { get; set; }
        public ResourceCollections ResourceCollections { get; set; }
    }
    public enum RequestType
    {
        BooksReservation, DVDReservation, CDReservation,
        EntertainmentPkgReservation, Search
    }
}
namespace LendingBranchServices
{
    using Roles;
    using LibrarySystem;
    public class LendingBranchServices : ISubject
    {
        public LendingBranchServices() { }
        public virtual bool CheckOut() { return true; }
        public virtual bool search(string callNum) { return true; }
    }
    public class SimpleServices : LendingBranchServices
    {
        Search _search;
        public SimpleServices() { _search = new Search(); }
        public override bool search(string callNum)
        {
            Console.WriteLine("SimpleServices search operation");
            return true;
        }
    }
    public class BooksReservation : SimpleServices, IContext
    {
        public override bool CheckOut()
        {
            Console.WriteLine("LoanableResource reservation operation");
            return true;
        }
        public IState State { get; set; }
    }
    public class Search
    {
        public Search() { }
        public bool search(string callNumber) 
        {
            Console.WriteLine("Search operation");
            return (true);
        }
    }
}
namespace LoanableResource
{
    using Roles;
    public class LoanableResource : IState
    {
        public string CallNumber { get; set; }
        public long DueDate { get; set; }
    }
    public class CheckedOut : LoanableResource
    {
        public CheckedOut() { Console.WriteLine("CheckedOut"); }
    }
    public class Available : LoanableResource
    {
        public Available() { Console.WriteLine("Available"); }
    }
}
namespace CaseStudy
{
    using Roles;
    using LibrarySystem;
    using LendingBranchServices;
    using LoanableResource;
    // Behavior Request() is injected into ISubject role so
    // now any object who assumes
    // this role, will have this method. Using
    // extension method to add "Request()"
    // method to objects involved in the request service
    public static class RequestTrait
    {
        public static bool Request(this IProxy proxy, ISubject subject, RequestType request)
        {
            bool rc = false;
            IContext ctxt = subject as IContext;
            IState book = ctxt.State;
            HandleBookReservationContext brContext = new HandleBookReservationContext(ctxt, book);
            switch (request)
            {
                case RequestType.BooksReservation:
                    rc = brContext.Doit();
                    break;
                case RequestType.Search:
                    LendingBranchServices ra = subject as LendingBranchServices;
                    string callNum = (book as LoanableResource).CallNumber;
                    rc = ra.search(callNum);
                    break;
                default:
                    Console.WriteLine("{0}: unrecognized request", request);
                    rc = false;
                    break;
            }
            return (rc);
        }
    }
    // Behavior Handle() is injected into State objects
    public static class HandleTrait
    {
        public static bool Handle(this IState state, IContext ctxt)
        {
            bool rc = false;
            Type tt = ctxt.State.GetType();
            string typeName = tt.ToString();
            LoanableResource book = ctxt.State as LoanableResource;
            switch (typeName)
            {
                case "LoanableResource.Available":
                    ctxt.State = new CheckedOut() { CallNumber = book.CallNumber, DueDate = book.DueDate };
                    rc = true;
                    break;
                case "LoanableResource.CheckedOut":
                    ctxt.State = new Available() { CallNumber = book.CallNumber, DueDate = book.DueDate };
                    break;       
                default: break;
            }
            return (rc);
        }
    }
    // Our collaboration model mimics the use case in DCI.
    // The context in which the RequestResource
    // (LoanableResource reservation) use case executed is this:
    public class RequestResourceContext
    {
        // properties for accessing the concrete objects
        // relevant in this context through their
        // methodless roles
        public IProxy Proxy { get; private set; }
        public ISubject Subject { get; private set; }
        public RequestType ReqType { get; private set; }

        public RequestResourceContext(ISubject subject, IProxy proxy,
            RequestType resource)
        {
            Proxy = proxy;
            Subject = subject;
            ReqType = resource;
        }
        // this is BookReservartion
        public bool Doit()
        {
            bool rc = Proxy.Request(Subject, ReqType);
            //IContext ctxt = this.Subject as IContext;
            //IState book = ctxt.State;            
            //HandleBookReservationContext brContext = new HandleBookReservationContext(ctxt, book);
            //bool rc = brContext.Doit();
            return (rc);
        }
    }
    public class HandleBookReservationContext
    {
        public IState State { get; private set; }
        public IContext Context { get; private set; }
        public HandleBookReservationContext(IContext ctxt, IState state)
        {
            State = state;
            Context = ctxt;
        }
        public bool Doit()
        {
            bool rc = State.Handle(Context);
            return (rc);
        }
    }
    class LibrarySystemCaseStudy
    {
        static void Main(string[] args)
        {
            // demonstrate Subject pattern integration
            Services services = new Services();
            SimpleServices ra = new BooksReservation() { State = new Available() { CallNumber = "123", DueDate = 12202013 } };
            RequestResourceContext integration = new RequestResourceContext(ra,services,RequestType.BooksReservation);
            bool rc = integration.Doit();
            rc = integration.Doit();
            integration = new RequestResourceContext(ra, services, RequestType.Search);
            rc = integration.Doit();
            Console.WriteLine("press any key to exit...");
            Console.ReadKey();
        }
    }
}
