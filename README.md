# QuickRest [![Build status](https://ci.appveyor.com/api/projects/status/h762wn0j57f5wvyx?svg=true)](https://ci.appveyor.com/project/xenoken/quickrest)


The simplest and quickest way to build and send REST requests.

## Getting Started

QuickRest is designed to allow developers build REST request very easily.

As an example, here is the most basic REST request achievable with QuickRest:

```
using QuickRest
Request r = new Request("http://www.ExampleWebsite.com");
r.Send();
```
and that's it! Your request has been sent successfully! Yes!

OK... but how do I get a response to my request?

With QuickRest, that's easy too.

Request.Send() is an asynchronus method, and will not block the program flow.

To get a response, subscribe to the *OnResponse* event of a request instance before sending it, and wait for the response to arrive.

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
  - By default, QuickRest uses the GET method for requests.
  - QuickRest targets .NET Framework 2.0.


## ChangeLog

  - Version *1.0*
  
        QuickRest First Version
