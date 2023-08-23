# GitOps - My Journey from Zero to Negative One

<!--


we can't have nive things


At the dawn of time, when times were harsh,
an APPLICATION
	runs on a MACHINE
=> "It works on my machine"

then, after decoupling the application from the machine,
an APPLICATION
	runs on an OS
		that operates a MACHINE
=> "It works on my OS"

because we still thought the application was too close from the machine (and because applications started having expectations about the underlying OS)
an APPLICATION
	runs on GUEST OS
		in a VM
			on a HOST OS
				that operates the MACHINE
=> "It works in my VM"

when virtual machines got too slow for our speed greed,
an APPLICATION
	runs on HALF AN OS
		isolated in a CONTAINER
			sharing its other half with a HOST OS
				that operates the MACHINE
=> "It works in my container"

but, thanks the gods, today,
an APPLICATION
	runs on HALF AN OS
		isolated in its CONTAINER
			wrapped by a POD
				part of a REPLICA SET
					managed by a DEPLOYMENT
						running on KUBERNETES-FIRST OS
							in a VM (you really didn't think they'd go away so easily)
								forming a CLUSTER
									provisioned in the CLOUD
=> "It works on my cluster"

Application release management has never been so streamlined.

-->
