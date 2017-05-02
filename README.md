# QuickRest

The simplest and quickest way to build and send REST requests.

## Getting Started

QuickRest is designed to help in building rest requests in the fastest way possible.

As an example, here is the most basic rest request achievable with quickrest:

```
using QuickRest
Request r = new Request("http://www.ExampleWebsite.com");
r.Send();
```
and that's it! Your request has been sent successfully! Yes!

OK... but how do I get an answer?

With QuickRest, that's easy too.

Request.Send() is an asynchronus method, and will not block the program flow.

To get an answer, subscribe to the *OnResponse* event of a request instance before sending it, and wait for the response to arrive.

It is that simple!

```
using QuickRest
Request r = new Request("http://www.ExampleWebsite.com");

'in order to get an answer for your request
r.OnResponse += OnResponseCallback;

r.Send();

...
public void OnResponseCallback(Request _req,Response _resp)
{
  //your_code_here
}


```

## Notes
  - By default QuickRest uses GET as the method of the request.
  - QuickRest targets .NET Framework 2.0.


## ChangeLog

  Version *1.0*
    QuickRest First Version
