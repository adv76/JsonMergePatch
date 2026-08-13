---
_layout: landing
---

# JSON Merge Patch

.NET has great JSON support through System.Text.Json but JSON PATCH support is very limited. This library adds [RFC 7396 JSON Merge Patch](https://datatracker.ietf.org/doc/html/rfc7396) support to .NET and ASP.NET Core.

## Why JSON Merge Patch

Sometimes you don't want to `PUT` the whole object just to update one property. ASP.NET Core has support for JSON Patch, but JSON patch is a lot more verbose and complicated. JSON merge patch, though a bit limited in some ways, is perfect for when you want to update a single or a couple of values.

Suppose you have this user object:


    {
        "id": 37,
        "firstName": "John",
        "lastName": "Doe",
        "email": "john@example.com",
        "phone": "1111111111"
    }


With JSON merge patch, updating John's last name is as simple as:

`HTTP PATCH`

    {
        "lastName": "Smith"
    }


This is super helpful when working on things such as multi-step forms on large entities and editable data-tables where the user only changes one value at a time.

