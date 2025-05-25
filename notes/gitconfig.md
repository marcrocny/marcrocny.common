# My Setup

Not much to it, really just shortcuts for everyday stuff. There's really no point in creating aliases for things that you do less often, like plumbing the reflog. Other things I do use more often, but want to type them out deliberately like `reset --hard` or `rebase --abort` or `clean -xdf`.

```ini
[user]
	name = Marc Lewandowski
	email = marcrocny@gmail.com
[core]
	autocrlf = true
	editor = vim
[alias]
	alias = config --get-regexp ^alias
	co = checkout
	cor = ! git fetch && git co
	edc = config --global -e
	cm = commit -m
	cam = ! git add . && git commit -am
	ca = commit --amend
	caa = commit -a --amend
	fwl = push --force-with-lease
	currentbranch = rev-parse --abbrev-ref HEAD
	pushset = "!f() { \
	  currentBranch=`git currentbranch` ; \
	  git push --set-upstream origin $currentBranch ; \
	}; f"
    # this needs to be adjusted for the specific remote URL format
	newpr = "!f() { \
	  currentBranch=`git currentbranch` ; \
	  url=`git remote get-url origin` ; \
	  start $url/prCreate?source=$currentBranch ; \
	}; f"
	rbc = rebase --continue
	rim = pull origin main
	cpm = ! git co main && git pull
[init]
	defaultBranch = main
[pull]
	rebase = interactive
```

If there's a common prefix-pattern for branch names on a job, sometimes I'll add that: `cob = co -b marc/$1` but that isn't always the case.

Another "sometimes" alias is `pp = fetch -pP` when the standard involves a lot of functional tagging (and therefore cleaning up of old, unused tags.)
