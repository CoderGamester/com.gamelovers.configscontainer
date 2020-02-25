# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [0.3.0] - 2020-02-25

- Added *ObservableDictionary*
- Added KeyValuePairTypes
- Added Vector3 extensions

## [0.2.0] - 2020-01-20

- Added *IdList* to help wrapping the access to a list of structs by a defined generic id

**Changed**: 
- Removed *IConfig*. Now the *ConfigsProvider* has an idResolver *Func* to resolve the map id to the config when adding configs to the provider

## [0.1.2] - 2020-01-09

**Fixed**: 
- Added missing meta files

## [0.1.1] - 2020-01-09

**Changed**: 
- Now *Configs* is *ConfigsProvider*
- Now *IConfigs* is *IConfigsProvider*

## [0.1.0] - 2020-01-06

- Initial submission for package distribution
