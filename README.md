NewsGet - Android
===================

Developed with **Xamarin.Android 6** using MS Visual Studio 2015

Here exists the source code for the main parts of <a href="http://newsget.in">NewsGet</a> for Android. This repository **DOES NOT** contain the actual Visual Studio project and OneSignal library used for push notifications.

Also missing is the server implementation of NewsGet, which is used to provide news content for both Android and the Web version of NewsGet.

NewsGet was a technology and entertainment news reader app for Android. Later for Web.


> **Notes:**

> - This project is fully compatible with Android 6.X Marshmallow
> - I made it in early 2016 when I wanted to learn Xamarin and strengthen my C# and Android framework skills.
> - I released it on <a>Cafebazaar</a> and <a>Myket</a>, two Iranian Android market.
> - General user impression was **very positive**. Average user given score was 4.7 on the biggest market and 5.0 on the other.
> - At least 1500 installs was confirmed.

I was disappointed with Xamarin on many aspects. Application size was huge. Even with all the possible linking options enabled. No matter what I did, the app always felt **heavy** to me. It wasn't as snappy as I expected. `OutOfMemoryError` was very common though some fixes were applied in the latest patch (v3.0.0). The intellisense provided on Visual Studio is much worse than the one provided for ASP.NET or WPF/WinForms apps. I would even say that Xamarin Studio 6+ is a better IDE for Xamarin. Weird build crashes and errors happen every now and then and sometimes debugging just doesn't want to work. Good luck getting XML (AXML) intellisense, to work (if there's even such a thing in Xamarin). Many useful and popular Android libraries don't have any proper binding. Ahh...
