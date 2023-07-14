# DHL CLI

Track your DHL shippings via CLI with current shipping events.

## 0. Setup

Set application's path in system's environment "Path" variables.

## 1. Using

##### Command tree of all (sub-) commands and parameters:
```
> dhl [command | alias] [subcommand | alias] [parameter]

[ number | -n ]
+--[ add | -a ]
|  +--[ number ]
+--[ remove | -r ]
   +--[ number ]

[ detail | -d ]
+--[ number ]

[ package | -p ]
+--[ number ]

[ update | -u ]

[ setkey | -s ]
+--[ key ]

[ getkey | -k ]

[ version | -v ]

[ help | -h ]
```

Type: <br>
`dhl help` or `dhl -h` for further information.