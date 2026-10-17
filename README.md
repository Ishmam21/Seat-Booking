# Seatz

Seatz is a seat reservation web app built with ASP.NET Core MVC. Vendors can create and edit seat layouts, and customers can browse layouts, choose seats, and complete a demo checkout.

## Live demo

[Open Seatz](https://seatz-h5ena4f4fxdrfwen.indiasouthcentral-01.azurewebsites.net)

## Features

- Browse published seating layouts and select available seats.
- Vendor accounts can create layouts, edit seat labels and prices, and upload seat preview images.
- 360 degree seat previews use Pannellum for compatible equirectangular images.
- Customers can book with an account or continue as a guest. Sign-in is optional.
- Guest checkout collects a name and email. Signed-in customers use their account details.
- Demo payment flow supports simulated success and failure; it does not process real payments.
- Customer accounts can view their bookings, with seats from the same checkout grouped together.
- Passwords are hashed with BCrypt. New and changed passwords require at least six characters, one number, and one special character.
- A Neon warm-up page checks database availability before opening the app.

## Technology

- .NET 10 and ASP.NET Core MVC
- Entity Framework Core
- PostgreSQL, hosted on Neon
- Pannellum 2.5.6 for 360 degree previews
- BCrypt.Net-Next for password hashing
- Razor, HTML, CSS, and JavaScript

## Run locally

You need the .NET 10 SDK and access to a PostgreSQL database with the Seatz schema.

Set the database connection string as a .NET user secret:

```sh
dotnet user-secrets set "ConnectionStrings:DbConn" "Host=YOUR_HOST;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require" --project AppLayerMVC/AppLayerMVC.csproj
```

The `bookings` table must support guest and grouped bookings. It needs a nullable `user_id`, plus `guest_name`, `guest_email`, `payment_status`, and `booking_group_id` columns. The `user_id` foreign key should allow null values. Keep database credentials out of source control.

Start the app:

```sh
dotnet run --project AppLayerMVC/AppLayerMVC.csproj
```

The root URL opens the warm-up page. The layouts page is available at `/Select/Index`.

## Demo payment

Checkout is for demonstration only. Choosing simulated payment success records the seat bookings; choosing failure leaves the seats unbooked. No payment provider or real transaction is involved.
