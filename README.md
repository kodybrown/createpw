createpw
========

A simple utility to create unique passwords from a key (such as a website name) and a master password.

The idea for the code came from [ss64.com](http://ss64.com/pass/).

    USAGE: createpw.exe [options] [key|site-name]

      key|site-name      Any unique key that you will remember.
                         If you’re using this for a website, put in
                         the url (such as: amazon or amazon.com).
                         If omitted, you will be prompted for it.

    OPTIONS:

      --clip             Put the newly created password onto the clipboard.
                         This is the default behavior.
      --no-clip          Output the new password to the console.
      --no-symbols       Do not output symbols in the new password.

When it asks for the user name, you can just press enter to bypass/skip it.
JUST be sure that you are consistent in supplying a user name with keys or not.

Here are a couple of examples:

```dos
> createpw amazon.com
Enter the username:         user
Enter the master password:  ********

password = <put into clipboard>
```

```dos
> createpw
Enter the site name or key: amazon.com
Enter the username:         user
Enter the master password:  ********

password = <put into clipboard>
```

```dos
> createpw --no-clip
Enter the site name or key: amazon.com
Enter the username:         user
Enter the master password:  ********

password = #ZJ~EEUmXuYVdMEMUK7CVbgxqY
```

I typed `12345678` for the passwords. In the last example, you should get the same result!
