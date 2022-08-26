# tretton37.crawler

This is a console application that can read, save and find new links from one page and do it again recursively

This application can process a lot of URLs concurrently (multithread).

I used **ConcurrentQueue** in my last approach but also I  implemented it by **Thread.Channel** in previous commits

This application traverse pages recursively so the progress percentage is not accurate because new links may add to queue runtime.

I didn't use IOC in this application because I want to be a simple console app